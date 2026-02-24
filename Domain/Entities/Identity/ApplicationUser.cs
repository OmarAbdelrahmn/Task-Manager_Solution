using Domain.Entities.Main;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Entities.Identity;

public class ApplicationUser : IdentityUser
{
    public override string? Email { get; set; }
    public override string? NormalizedEmail { get; set; }
    public string? FullName { get; set; } = string.Empty;

    public string? Address { get; set; } = string.Empty;

    public DateTime? LastLogin { get; set; }

    public string? AvatarUrl { get; set; }
    public bool IsOnline { get; set; }
    public bool IsDisabled { get; set; }

    public List<RefreshToken> RefreshTokens { get; set; } = [];
    public ICollection<TaskAssignee> AssignedTasks { get; set; } = [];
    public ICollection<ConversationParticipant> Conversations { get; set; } = [];
    public ICollection<Message> Messages { get; set; } = [];
    public ICollection<Notification> Notifications { get; set; } = [];
}
