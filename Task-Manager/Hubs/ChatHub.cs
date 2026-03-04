
using Application.Extensions;
using Application.Services.Conversations;
using Domain;
using Domain.Entities.Main;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Task_Manager.Hubs;

[Authorize]   // requires valid JWT
public class ChatHub(ApplicationDbcontext db) : Hub
{
    // Called automatically by SignalR when a client connects
    public override async Task OnConnectedAsync()
    {
        var userId = Context.User!.GetUserId();

        // Add the connection to every conversation group this user belongs to
        var convIds = await db.Set<ConversationParticipant>()
            .Where(p => p.UserId == userId)
            .Select(p => p.ConversationId)
            .ToListAsync();

        foreach (var id in convIds)
            await Groups.AddToGroupAsync(Context.ConnectionId, $"conversation-{id}");

        // Mark user online
        var user = await db.Users.FindAsync(userId);
        if (user is not null)
        {
            user.IsOnline = true;
            await db.SaveChangesAsync();
        }

        // Notify others that this user is online
        await Clients.Others.SendAsync("UserOnline", userId);

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User!.GetUserId();

        var user = await db.Users.FindAsync(userId);
        if (user is not null)
        {
            user.IsOnline = false;
            await db.SaveChangesAsync();
        }

        await Clients.Others.SendAsync("UserOffline", userId);

        await base.OnDisconnectedAsync(exception);
    }

    // ── Client can join a specific conversation group manually ──────
    // Useful after creating a new conversation without reconnecting
    public async Task JoinConversation(int conversationId)
    {
        var userId = Context.User!.GetUserId();

        var isParticipant = await db.Set<ConversationParticipant>()
            .AnyAsync(p => p.ConversationId == conversationId && p.UserId == userId);

        if (!isParticipant)
        {
            await Clients.Caller.SendAsync("Error", "You are not a participant of this conversation.");
            return;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, $"conversation-{conversationId}");
        await Clients.Caller.SendAsync("JoinedConversation", conversationId);
    }

    public async Task LeaveConversation(int conversationId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"conversation-{conversationId}");
    }

    // ── Typing indicators ────────────────────────────────────────────
    public async Task TypingStart(int conversationId)
    {
        var userId = Context.User!.GetUserId();
        await Clients.OthersInGroup($"conversation-{conversationId}")
            .SendAsync("UserTyping", new { userId, conversationId });
    }

    public async Task TypingStop(int conversationId)
    {
        var userId = Context.User!.GetUserId();
        await Clients.OthersInGroup($"conversation-{conversationId}")
            .SendAsync("UserStoppedTyping", new { userId, conversationId });
    }
}
