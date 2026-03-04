using Application.Abstraction;
using Application.Abstraction.Errors;
using Application.Contracts.Conversations;
using Domain;
using Domain.Entities;
using Domain.Entities.Identity;
using Domain.Entities.Main;
using global::Application.Contracts.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.Conversations;

public class ConversationService(
    ApplicationDbcontext db,
    UserManager<ApplicationUser> userManager,
    IWebHostEnvironment env,
    IHubContext<ChatHub> hub          // inject SignalR hub context
) : IConversationService
{
    // ── GET MY CONVERSATIONS ─────────────────────────────────────────
    public async Task<Result<IEnumerable<ConversationSummaryResponse>>> GetMyConversationsAsync(string userId)
    {
        var conversations = await db.Conversations
            .Where(c => !c.IsDeleted &&
                        c.Participants.Any(p => p.UserId == userId))
            .Include(c => c.Participants).ThenInclude(p => p.User)
            .Include(c => c.Messages.OrderByDescending(m => m.CreatedAt).Take(1))
                .ThenInclude(m => m.Sender)
            .AsNoTracking()
            .OrderByDescending(c => c.Messages
                .OrderByDescending(m => m.CreatedAt)
                .Select(m => (DateTime?)m.CreatedAt)
                .FirstOrDefault() ?? c.CreatedAt)
            .ToListAsync();

        var result = conversations.Select(c => MapToSummary(c, userId)).ToList();
        return Result.Success<IEnumerable<ConversationSummaryResponse>>(result);
    }

    // ── GET SINGLE CONVERSATION ──────────────────────────────────────
    public async Task<Result<ConversationSummaryResponse>> GetConversationAsync(int conversationId, string userId)
    {
        var conv = await LoadConversationAsync(conversationId);

        if (conv is null || conv.IsDeleted)
            return Result.Failure<ConversationSummaryResponse>(ConversationErrors.NotFound);

        if (!conv.Participants.Any(p => p.UserId == userId))
            return Result.Failure<ConversationSummaryResponse>(ConversationErrors.NotParticipant);

        return Result.Success(MapToSummary(conv, userId));
    }

    // ── CREATE DIRECT ────────────────────────────────────────────────
    public async Task<Result<ConversationSummaryResponse>> CreateDirectAsync(
        CreateDirectConversationRequest req, string creatorId)
    {
        if (req.TargetUserId == creatorId)
            return Result.Failure<ConversationSummaryResponse>(ConversationErrors.CannotDirectWithSelf);

        if (await userManager.FindByIdAsync(req.TargetUserId) is null)
            return Result.Failure<ConversationSummaryResponse>(UserErrors.UserNotFound);

        // Check if a direct conversation already exists between these two
        var existing = await db.Conversations
            .Where(c => c.Type == ConversationType.Direct && !c.IsDeleted &&
                        c.Participants.Any(p => p.UserId == creatorId) &&
                        c.Participants.Any(p => p.UserId == req.TargetUserId))
            .FirstOrDefaultAsync();

        if (existing is not null)
        {
            var loaded = await LoadConversationAsync(existing.Id);
            return Result.Success(MapToSummary(loaded!, creatorId));
        }

        var conv = new Conversation
        {
            Type = ConversationType.Direct,
            CreatedById = creatorId,
            Participants =
            [
                new ConversationParticipant { UserId = creatorId },
                new ConversationParticipant { UserId = req.TargetUserId }
            ]
        };

        db.Conversations.Add(conv);
        await db.SaveChangesAsync();

        var created = await LoadConversationAsync(conv.Id);
        return Result.Success(MapToSummary(created!, creatorId));
    }

    // ── CREATE GROUP ─────────────────────────────────────────────────
    public async Task<Result<ConversationSummaryResponse>> CreateGroupAsync(
        CreateGroupConversationRequest req, string creatorId)
    {
        if (string.IsNullOrWhiteSpace(req.Name))
            return Result.Failure<ConversationSummaryResponse>(ConversationErrors.GroupNameRequired);

        var allIds = req.MemberIds.Distinct().Append(creatorId).ToList();
        var participants = new List<ConversationParticipant>();

        foreach (var id in allIds)
        {
            if (await userManager.FindByIdAsync(id) is null) continue;
            participants.Add(new ConversationParticipant { UserId = id });
        }

        var conv = new Conversation
        {
            Type = ConversationType.Group,
            Name = req.Name.Trim(),
            CreatedById = creatorId,
            Participants = participants
        };

        db.Conversations.Add(conv);
        await db.SaveChangesAsync();

        var created = await LoadConversationAsync(conv.Id);
        return Result.Success(MapToSummary(created!, creatorId));
    }

    // ── ADD / REMOVE PARTICIPANT ─────────────────────────────────────
    public async Task<Result> AddParticipantAsync(int conversationId, string userId, string addedById)
    {
        var conv = await db.Conversations
            .Include(c => c.Participants)
            .FirstOrDefaultAsync(c => c.Id == conversationId && !c.IsDeleted);

        if (conv is null) return Result.Failure(ConversationErrors.NotFound);
        if (conv.Type != ConversationType.Group)
            return Result.Failure(ConversationErrors.OnlyGroupsAllowParticipantChange);
        if (!conv.Participants.Any(p => p.UserId == addedById))
            return Result.Failure(ConversationErrors.NotParticipant);
        if (conv.Participants.Any(p => p.UserId == userId))
            return Result.Failure(ConversationErrors.AlreadyParticipant);
        if (await userManager.FindByIdAsync(userId) is null)
            return Result.Failure(UserErrors.UserNotFound);

        conv.Participants.Add(new ConversationParticipant { UserId = userId });
        await db.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result> RemoveParticipantAsync(int conversationId, string userId, string removedById)
    {
        var conv = await db.Conversations
            .Include(c => c.Participants)
            .FirstOrDefaultAsync(c => c.Id == conversationId && !c.IsDeleted);

        if (conv is null) return Result.Failure(ConversationErrors.NotFound);
        if (conv.Type != ConversationType.Group)
            return Result.Failure(ConversationErrors.OnlyGroupsAllowParticipantChange);

        // Only creator or the user themselves can remove
        if (removedById != conv.CreatedById && removedById != userId)
            return Result.Failure(ConversationErrors.NotAuthorized);

        var participant = conv.Participants.FirstOrDefault(p => p.UserId == userId);
        if (participant is null) return Result.Failure(ConversationErrors.NotParticipant);

        conv.Participants.Remove(participant);
        await db.SaveChangesAsync();
        return Result.Success();
    }

    // ── MARK AS READ ─────────────────────────────────────────────────
    public async Task<Result> MarkAsReadAsync(int conversationId, string userId)
    {
        var participant = await db.Set<ConversationParticipant>()
            .FirstOrDefaultAsync(p => p.ConversationId == conversationId && p.UserId == userId);

        if (participant is null)
            return Result.Failure(ConversationErrors.NotParticipant);

        participant.LastReadAt = DateTime.Now;
        await db.SaveChangesAsync();
        return Result.Success();
    }

    // ── GET MESSAGES (paged, newest-last) ────────────────────────────
    public async Task<PagedResponse<MessageResponse>> GetMessagesAsync(
        int conversationId, string userId, int page = 1, int pageSize = 50)
    {
        var isParticipant = await db.Set<ConversationParticipant>()
            .AnyAsync(p => p.ConversationId == conversationId && p.UserId == userId);

        if (!isParticipant)
            return new PagedResponse<MessageResponse>([], 0, page, pageSize, 0);

        var query = db.Messages
            .Where(m => m.ConversationId == conversationId)
            .AsNoTracking();

        var total = await query.CountAsync();
        pageSize = Math.Clamp(pageSize, 1, 100);
        page = Math.Max(page, 1);

        var messages = await query
            .Include(m => m.Sender)
            .Include(m => m.Files)
            .Include(m => m.ReplyTo).ThenInclude(r => r!.Sender)
            .OrderBy(m => m.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResponse<MessageResponse>(
            messages.Select(MapToMessageResponse).ToList(),
            total, page, pageSize,
            (int)Math.Ceiling((double)total / pageSize));
    }

    // ── SEND MESSAGE ─────────────────────────────────────────────────
    public async Task<Result<MessageResponse>> SendMessageAsync(SendMessageRequest req, string senderId)
    {
        var conv = await db.Conversations
            .Include(c => c.Participants)
            .FirstOrDefaultAsync(c => c.Id == req.ConversationId && !c.IsDeleted);

        if (conv is null)
            return Result.Failure<MessageResponse>(ConversationErrors.NotFound);

        if (!conv.Participants.Any(p => p.UserId == senderId))
            return Result.Failure<MessageResponse>(ConversationErrors.NotParticipant);

        if (string.IsNullOrWhiteSpace(req.Body) && req.Type == MessageType.Text)
            return Result.Failure<MessageResponse>(ConversationErrors.EmptyMessage);

        if (req.ReplyToId.HasValue)
        {
            var replyExists = await db.Messages
                .AnyAsync(m => m.Id == req.ReplyToId.Value && m.ConversationId == req.ConversationId);
            if (!replyExists)
                return Result.Failure<MessageResponse>(ConversationErrors.ReplyTargetNotFound);
        }

        var msg = new Message
        {
            ConversationId = req.ConversationId,
            SenderId = senderId,
            Body = req.Body?.Trim(),
            Type = req.Type,
            ReplyToId = req.ReplyToId
        };

        db.Messages.Add(msg);
        await db.SaveChangesAsync();

        // Reload with navigation props for SignalR broadcast
        var full = await db.Messages
            .Include(m => m.Sender)
            .Include(m => m.Files)
            .Include(m => m.ReplyTo).ThenInclude(r => r!.Sender)
            .AsNoTracking()
            .FirstAsync(m => m.Id == msg.Id);

        var response = MapToMessageResponse(full);

        // ── Broadcast to SignalR group ───────────────────────────────
        await hub.Clients
            .Group($"conversation-{req.ConversationId}")
            .SendAsync("ReceiveMessage", response);

        return Result.Success(response);
    }

    // ── EDIT MESSAGE ─────────────────────────────────────────────────
    public async Task<Result<MessageResponse>> EditMessageAsync(
        int messageId, EditMessageRequest req, string editorId)
    {
        var msg = await db.Messages
            .Include(m => m.Sender)
            .Include(m => m.Files)
            .FirstOrDefaultAsync(m => m.Id == messageId && !m.IsDeleted);

        if (msg is null)
            return Result.Failure<MessageResponse>(ConversationErrors.MessageNotFound);

        if (msg.SenderId != editorId)
            return Result.Failure<MessageResponse>(ConversationErrors.NotAuthorized);

        if (msg.Type != MessageType.Text)
            return Result.Failure<MessageResponse>(ConversationErrors.CannotEditNonText);

        msg.Body = req.NewBody.Trim();
        msg.IsEdited = true;
        await db.SaveChangesAsync();

        var response = MapToMessageResponse(msg);

        await hub.Clients
            .Group($"conversation-{msg.ConversationId}")
            .SendAsync("MessageEdited", response);

        return Result.Success(response);
    }

    // ── DELETE MESSAGE (soft) ────────────────────────────────────────
    public async Task<Result> DeleteMessageAsync(int messageId, string deletedById)
    {
        var msg = await db.Messages
            .FirstOrDefaultAsync(m => m.Id == messageId && !m.IsDeleted);

        if (msg is null)
            return Result.Failure(ConversationErrors.MessageNotFound);

        if (msg.SenderId != deletedById)
            return Result.Failure(ConversationErrors.NotAuthorized);

        msg.IsDeleted = true;
        msg.Body = null;   // wipe content
        await db.SaveChangesAsync();

        // Notify clients that message was deleted
        await hub.Clients
            .Group($"conversation-{msg.ConversationId}")
            .SendAsync("MessageDeleted", new { messageId, conversationId = msg.ConversationId });

        return Result.Success();
    }

    // ── UPLOAD MESSAGE FILE ──────────────────────────────────────────
    public async Task<Result<MessageFileResponse>> UploadMessageFileAsync(
        int conversationId, IFormFile file, string uploadedById)
    {
        var isParticipant = await db.Set<ConversationParticipant>()
            .AnyAsync(p => p.ConversationId == conversationId && p.UserId == uploadedById);

        if (!isParticipant)
            return Result.Failure<MessageFileResponse>(ConversationErrors.NotParticipant);

        var storedName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var uploadFolder = Path.Combine(env.WebRootPath, "uploads", "messages", conversationId.ToString());
        Directory.CreateDirectory(uploadFolder);

        var physicalPath = Path.Combine(uploadFolder, storedName);
        await using (var stream = new FileStream(physicalPath, FileMode.Create))
            await file.CopyToAsync(stream);

        // Caller should then send a message with Type=File referencing the returned URL
        return Result.Success(new MessageFileResponse(
            0,
            file.FileName,
            $"/uploads/messages/{conversationId}/{storedName}",
            file.ContentType,
            file.Length));
    }

    // ═══════════════════════════════════════════════════════════════
    //  PRIVATE HELPERS
    // ═══════════════════════════════════════════════════════════════

    private async Task<Conversation?> LoadConversationAsync(int id) =>
        await db.Conversations
            .Include(c => c.Participants).ThenInclude(p => p.User)
            .Include(c => c.Messages.OrderByDescending(m => m.CreatedAt).Take(1))
                .ThenInclude(m => m.Sender)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);

    private static ConversationSummaryResponse MapToSummary(Conversation c, string currentUserId)
    {
        var lastMsg = c.Messages.OrderByDescending(m => m.CreatedAt).FirstOrDefault();
        var myLastRead = c.Participants
            .FirstOrDefault(p => p.UserId == currentUserId)?.LastReadAt;

        var unread = myLastRead.HasValue
            ? c.Messages.Count(m => m.CreatedAt > myLastRead.Value && m.SenderId != currentUserId)
            : 0;

        return new ConversationSummaryResponse(
            c.Id, c.Type, c.Name, c.TaskId,
            c.Participants.Select(MapToParticipant).ToList(),
            lastMsg is not null ? MapToMessageResponse(lastMsg) : null,
            unread,
            c.CreatedAt);
    }

    private static ParticipantResponse MapToParticipant(ConversationParticipant p) => new(
        p.UserId,
        p.User?.UserName ?? "",
        p.User?.FullName ?? "",
        p.User?.AvatarUrl,
        p.User?.IsOnline ?? false,
        p.JoinedAt,
        p.LastReadAt);

    private static MessageResponse MapToMessageResponse(Message m) => new(
        m.Id, m.ConversationId,
        m.SenderId,
        m.Sender?.UserName ?? "",
        m.Sender?.AvatarUrl,
        m.IsDeleted ? null : m.Body,
        m.Type,
        m.ReplyToId,
        m.ReplyTo is not null ? MapToMessageResponse(m.ReplyTo) : null,
        m.IsEdited, m.IsDeleted,
        m.Files.Select(f => new MessageFileResponse(f.Id, f.FileName, f.FileUrl, f.MimeType, f.FileSize)).ToList(),
        m.CreatedAt);
}
