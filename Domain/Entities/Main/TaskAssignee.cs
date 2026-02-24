using Domain.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Entities.Main;

public class TaskAssignee
{
    public int TaskId { get; set; }
    public string UserId { get; set; } = string.Empty;      // who is assigned
    public string AssignedById { get; set; } = string.Empty; // who did the assigning
                                                             // UserId == AssignedById = self-assigned
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

    // Nav
    public AppTask Task { get; set; } = default!;
    public ApplicationUser User { get; set; } = default!;
    public ApplicationUser AssignedBy { get; set; } = default!;
}