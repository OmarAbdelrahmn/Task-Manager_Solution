using Domain.Entities.Base;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Formats.Tar;
using System.Text;

namespace Domain.Entities.Main;


public class AppTask : AuditableEntity
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TaskStatus Status { get; set; } = TaskStatus.Todo;
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    public int Progress { get; set; } = 0;           // 0–100
    public DateTime? DueDate { get; set; }

    // Recurrence
    public bool IsRecurring { get; set; } = false;
    public RecurrenceType? RecurrenceType { get; set; }
    public int? RecurrenceInterval { get; set; }      // every X months/weeks
    public DateTime? RecurrenceStartDate { get; set; }
    public DateTime? RecurrenceEndDate { get; set; }  // null = forever

    // Nav
    public ICollection<TaskAssignee> Assignees { get; set; } = [];
    public ICollection<TaskFile> Files { get; set; } = [];
    public ICollection<TaskOccurrence> Occurrences { get; set; } = [];
    public Conversation? Conversation { get; set; }



public void RecalculateStatus(AppTask task)
    {
        task.Status = task.Progress switch
        {
            0 => TaskStatus.Todo,
            100 => TaskStatus.Done,
            _ => TaskStatus.InProgress   // anything between 1–99
        };
    }

}