using Application.Abstraction;
using Application.Contracts.Conversations;
using Application.Contracts.Tasks;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Services.Conversations;


public interface IConversationService
{
    // ── Conversations ────────────────────────────────────────────────
    Task<Result<IEnumerable<ConversationSummaryResponse>>> GetMyConversationsAsync(string userId);
    Task<Result<ConversationSummaryResponse>> GetConversationAsync(int conversationId, string userId);
    Task<Result<ConversationSummaryResponse>> CreateDirectAsync(CreateDirectConversationRequest req, string creatorId);
    Task<Result<ConversationSummaryResponse>> CreateGroupAsync(CreateGroupConversationRequest req, string creatorId);
    Task<Result> AddParticipantAsync(int conversationId, string userId, string addedById);
    Task<Result> RemoveParticipantAsync(int conversationId, string userId, string removedById);
    Task<Result> MarkAsReadAsync(int conversationId, string userId);

    // ── Messages ────────────────────────────────────────────────────
    Task<PagedResponse<MessageResponse>> GetMessagesAsync(int conversationId, string userId, int page = 1, int pageSize = 50);
    Task<Result<MessageResponse>> SendMessageAsync(SendMessageRequest req, string senderId);
    Task<Result<MessageResponse>> EditMessageAsync(int messageId, EditMessageRequest req, string editorId);
    Task<Result> DeleteMessageAsync(int messageId, string deletedById);
    Task<Result<MessageFileResponse>> UploadMessageFileAsync(int conversationId, IFormFile file, string uploadedById);
}