using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Abstraction.Errors;

public static class ConversationErrors
{
    public static readonly Error NotFound = new("Conversation.NotFound", "Conversation not found.", 404);
    public static readonly Error NotParticipant = new("Conversation.NotParticipant", "You are not part of this conversation.", 403);
    public static readonly Error NotAuthorized = new("Conversation.NotAuthorized", "You are not allowed to do this.", 403);
    public static readonly Error AlreadyParticipant = new("Conversation.AlreadyIn", "User is already a participant.", 400);
    public static readonly Error CannotDirectWithSelf = new("Conversation.SelfDirect", "Cannot start a conversation with yourself.", 400);
    public static readonly Error GroupNameRequired = new("Conversation.NoName", "Group name is required.", 400);
    public static readonly Error OnlyGroupsAllowParticipantChange = new("Conversation.NotGroup", "Only group conversations support this.", 400);
    public static readonly Error EmptyMessage = new("Message.Empty", "Message body cannot be empty.", 400);
    public static readonly Error MessageNotFound = new("Message.NotFound", "Message not found.", 404);
    public static readonly Error CannotEditNonText = new("Message.NotEditable", "Only text messages can be edited.", 400);
    public static readonly Error ReplyTargetNotFound = new("Message.ReplyNotFound", "Replied-to message not found.", 404);
}
