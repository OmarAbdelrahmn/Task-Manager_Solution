using Domain.Entities.Base;
using Domain.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Entities.Main;

public class Message : BaseEntity
{
    public int ConversationId { get; set; }
    public string SenderId { get; set; } = string.Empty;
    public string? Body { get; set; }
    public MessageType Type { get; set; } = MessageType.Text;
    public int? ReplyToId { get; set; }
    public bool IsEdited { get; set; } = false;
    public bool IsDeleted { get; set; } = false;       // soft delete for messages

    // Nav
    public Conversation Conversation { get; set; } = default!;
    public ApplicationUser Sender { get; set; } = default!;
    public Message? ReplyTo { get; set; }
    public ICollection<MessageFile> Files { get; set; } = [];
    public ICollection<MessageReaction> Reactions { get; set; } = [];
}