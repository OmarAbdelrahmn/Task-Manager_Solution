using Domain.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Entities.Main;


public class ConversationParticipant
{
    public int ConversationId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastReadAt { get; set; }          // for unread badge

    // Nav
    public Conversation Conversation { get; set; } = default!;
    public ApplicationUser User { get; set; } = default!;
}