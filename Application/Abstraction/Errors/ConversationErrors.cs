using Microsoft.AspNetCore.Http;
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
    public static readonly Error MessingMembers = new("Messing.Members", "the members for this conversation is not founds.", 404);
    public static readonly Error CannotDeleteDirect =
       new("Conversation.CannotDeleteDirect",
           "Direct conversations cannot be deleted, only left.",
           StatusCodes.Status400BadRequest);

    public static readonly Error CannotLeaveTaskThread =
        new("Conversation.CannotLeaveTaskThread",
            "Task-thread conversations are managed automatically.",
            StatusCodes.Status400BadRequest);

    public static readonly Error CannotLeaveAsLastParticipant =
        new("Conversation.CannotLeaveAsLastParticipant",
            "You are the last participant. Delete the conversation instead.",
            StatusCodes.Status400BadRequest);

    public static readonly Error ReactionAlreadyExists =
        new("Reaction.AlreadyExists",
            "You have already reacted with this emoji.",
            StatusCodes.Status409Conflict);

    public static readonly Error ReactionNotFound =
        new("Reaction.NotFound",
            "Reaction not found.",
            StatusCodes.Status404NotFound);

    public static readonly Error InvalidEmoji =
        new("Reaction.InvalidEmoji",
            "The emoji provided is not allowed.",
            StatusCodes.Status400BadRequest);
}
