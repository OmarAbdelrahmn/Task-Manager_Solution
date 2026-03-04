using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Contracts.Conversations;


public record CreateDirectConversationRequest(string TargetUserId);

public record CreateGroupConversationRequest(
    string Name,
    List<string> MemberIds);   // excludes creator — added automatically

public record SendMessageRequest(
    int ConversationId,
    string? Body,
    MessageType Type = MessageType.Text,
    int? ReplyToId = null);

public record EditMessageRequest(string NewBody);

// ── Responses ────────────────────────────────────────────────────────

public record ConversationSummaryResponse(
    int Id,
    ConversationType Type,
    string? Name,
    int? TaskId,
    List<ParticipantResponse> Participants,
    MessageResponse? LastMessage,
    int UnreadCount,
    DateTime CreatedAt);

public record MessageResponse(
    int Id,
    int ConversationId,
    string SenderId,
    string SenderName,
    string? SenderAvatar,
    string? Body,
    MessageType Type,
    int? ReplyToId,
    MessageResponse? ReplyTo,      // hydrated inline for UI
    bool IsEdited,
    bool IsDeleted,
    List<MessageFileResponse> Files,
    DateTime CreatedAt);

public record MessageFileResponse(
    int Id,
    string FileName,
    string FileUrl,
    string MimeType,
    long FileSize);

public record ParticipantResponse(
    string UserId,
    string UserName,
    string FullName,
    string? AvatarUrl,
    bool IsOnline,
    DateTime JoinedAt,
    DateTime? LastReadAt);

