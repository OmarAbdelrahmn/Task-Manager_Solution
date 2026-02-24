using Domain.Entities.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Entities.Main;


public class TaskFile : BaseEntity
{
    public int TaskId { get; set; }
    public int? OccurrenceId { get; set; }             // null = main task file
    public string FileName { get; set; } = string.Empty;
    public string StoredFileName { get; set; } = string.Empty; // the GUID name on disk
    public string FileUrl { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string MimeType { get; set; } = string.Empty;

    // Nav
    public AppTask Task { get; set; } = default!;
    public TaskOccurrence? Occurrence { get; set; }
}