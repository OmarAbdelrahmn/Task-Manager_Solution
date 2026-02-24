using Domain.Entities.Base;
using System;
using System.Collections.Generic;
using System.Formats.Tar;
using System.Text;

namespace Domain.Entities.Main;

public class TaskOccurrence : BaseEntity
{
    public int TaskId { get; set; }
    public DateTime DueDate { get; set; }
    public TaskStatus Status { get; set; } = TaskStatus.Todo;
    public int Progress { get; set; } = 0;
    public string? Notes { get; set; }
    public DateTime? CompletedAt { get; set; }

    // Nav
    public AppTask Task { get; set; } = default!;
    public ICollection<TaskFile> Files { get; set; } = [];
}