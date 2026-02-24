using Domain.Entities.Base;
using Domain.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Entities.Main;

public class Notification : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public int? ReferenceId { get; set; }
    public string? ReferenceType { get; set; }         // "Task", "Message", etc.
    public bool IsRead { get; set; } = false;

    // Nav
    public ApplicationUser User { get; set; } = default!;
}