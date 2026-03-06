// Application/Services/Conversations/IConversationService.cs
using Application.Abstraction;
using Application.Contracts.Conversations;
using Application.Contracts.Tasks;
using Application.Services.UserFile;
using Microsoft.AspNetCore.Http;

namespace Application.Services.Conversations;

public interface IConversationService
{
    // ── Conversations ──────────────────────────────────────────────────
    Task<Result<IEnumerable<ConversationSummaryResponse>>> GetMyConversationsAsync(string userId);
    Task<Result<ConversationSummaryResponse>> GetConversationAsync(int conversationId, string userId);
    Task<Result<ConversationSummaryResponse>> CreateDirectAsync(CreateDirectConversationRequest req, string creatorId);
    Task<Result<ConversationSummaryResponse>> CreateGroupAsync(CreateGroupConversationRequest req, string creatorId);
    Task<Result<ConversationSummaryResponse>> UpdateGroupAsync(int conversationId, UpdateGroupRequest req, string requesterId);
    Task<Result> DeleteConversationAsync(int conversationId, string requesterId);
    Task<Result> LeaveConversationAsync(int conversationId, string userId);
    Task<Result> AddParticipantAsync(int conversationId, string userId, string addedById);
    Task<Result> RemoveParticipantAsync(int conversationId, string userId, string removedById);
    Task<Result> MarkAsReadAsync(int conversationId, string userId);
    Task<Result<UnreadCountResponse>> GetUnreadCountAsync(string userId);

    // ── Messages ──────────────────────────────────────────────────────
    Task<PagedResponse<MessageResponse>> GetMessagesAsync(int conversationId, string userId, int page = 1, int pageSize = 50);
    Task<PagedResponse<MessageResponse>> SearchMessagesAsync(int conversationId, string userId, MessageSearchRequest req);
    Task<Result<MessageResponse>> SendMessageAsync(SendMessageRequest req, string senderId);
    Task<Result<MessageResponse>> EditMessageAsync(int messageId, EditMessageRequest req, string editorId);
    Task<Result> DeleteMessageAsync(int messageId, string deletedById);
    Task<Result<MessageFileResponse>> UploadMessageFileAsync(int conversationId, IFormFile file, string uploadedById);

    // ── Reactions ─────────────────────────────────────────────────────
    Task<Result<IEnumerable<MessageReactionResponse>>> GetReactionsAsync(int messageId, string userId);
    Task<Result<MessageReactionResponse>> ReactAsync(int messageId, ReactRequest req, string userId);
    Task<Result> RemoveReactionAsync(int messageId, string emoji, string userId);

    Task<Result<string>> UpdateConversationAvatarAsync(
    int conversationId, IFormFile file, string requesterId, IUserFileService fileService);
}