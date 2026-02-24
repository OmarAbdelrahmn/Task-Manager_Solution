using Domain.Entities.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Entities.Main;

public class Conversation : AuditableEntity
{
    public ConversationType Type { get; set; }
    public string? Name { get; set; }                  // for group chats only
    public int? TaskId { get; set; }                   // only when Type = TaskThread

    // Nav
    public AppTask? Task { get; set; }
    public ICollection<ConversationParticipant> Participants { get; set; } = [];
    public ICollection<Message> Messages { get; set; } = [];
}