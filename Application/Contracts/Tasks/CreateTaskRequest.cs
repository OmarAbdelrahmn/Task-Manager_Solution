// Application/Contracts/Tasks/TaskContracts.cs
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using TaskStatus = Domain.Entities.TaskStatus;

namespace Application.Contracts.Tasks;

// ─────────────────────────────────────────────
//  REQUESTS
// ─────────────────────────────────────────────

/// <summary>Create a new task (recurring or one-off).</summary>
public record CreateTaskRequest(
    string Title,
    string? Description,
    TaskPriority Priority,
    DateTime? DueDate,

    // Recurrence (all required together when IsRecurring = true)
    bool IsRecurring,
    RecurrenceType? RecurrenceType,
    int? RecurrenceInterval,
    DateTime? RecurrenceStartDate,
    DateTime? RecurrenceEndDate,

    // Optional: assign users immediately upon creation
    List<string>? AssigneeIds
);

/// <summary>Update the core fields of a task.</summary>
public record UpdateTaskRequest(
    string Title,
    string? Description,
    TaskPriority Priority,
    DateTime? DueDate,

    bool IsRecurring,
    RecurrenceType? RecurrenceType,
    int? RecurrenceInterval,
    DateTime? RecurrenceStartDate,
    DateTime? RecurrenceEndDate
);

/// <summary>Update only the progress (0–100). Status is recalculated automatically.</summary>
public record UpdateProgressRequest(int Progress);

/// <summary>Assign a user to a task.</summary>
public record AssignUserRequest(string UserId);

/// <summary>Update an occurrence's progress/notes.</summary>
public record UpdateOccurrenceRequest(
    int Progress,
    string? Notes
);

/// <summary>Filters + pagination for the task list endpoint.</summary>
public record TaskFilterRequest(
    string? Search,
    TaskStatus? Status,
    TaskPriority? Priority,
    string? AssignedUserId,   // filter tasks assigned to a specific user
    bool? IsRecurring,
    DateTime? DueBefore,
    DateTime? DueAfter,
    bool IncludeDeleted = false,
    int Page = 1,
    int PageSize = 20
);

// ─────────────────────────────────────────────
//  RESPONSES
// ─────────────────────────────────────────────

/// <summary>Lightweight row used in list views.</summary>
public record TaskSummaryResponse(
    int Id,
    string Title,
    TaskStatus Status,
    TaskPriority Priority,
    int Progress,
    DateTime? DueDate,
    bool IsRecurring,
    DateTime CreatedAt,
    string? CreatedById,
    bool IsDeleted,
    List<AssigneeSummary> Assignees
);

/// <summary>Full task detail including files, occurrences, and conversation info.</summary>
public record TaskDetailResponse(
    int Id,
    string Title,
    string? Description,
    TaskStatus Status,
    TaskPriority Priority,
    int Progress,
    DateTime? DueDate,
    bool IsRecurring,
    RecurrenceType? RecurrenceType,
    int? RecurrenceInterval,
    DateTime? RecurrenceStartDate,
    DateTime? RecurrenceEndDate,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    string? CreatedById,
    string? UpdatedById,
    bool IsDeleted,
    DateTime? DeletedAt,
    List<AssigneeSummary> Assignees,
    List<TaskFileResponse> Files,
    List<TaskOccurrenceResponse> Occurrences,
    int? ConversationId         // linked task-thread conversation, if any
);

/// <summary>Slim assignee info embedded inside task responses.</summary>
public record AssigneeSummary(
    string UserId,
    string UserName,
    string FullName,
    string? AvatarUrl,
    string AssignedById,
    DateTime AssignedAt
);

/// <summary>File attached to a task or occurrence.</summary>
public record TaskFileResponse(
    int Id,
    string FileName,
    string FileUrl,
    string MimeType,
    long FileSize,
    int? OccurrenceId,          // null = main task file
    DateTime CreatedAt,
    string? CreatedById
);

/// <summary>A single occurrence of a recurring task.</summary>
public record TaskOccurrenceResponse(
    int Id,
    DateTime DueDate,
    TaskStatus Status,
    int Progress,
    string? Notes,
    DateTime? CompletedAt,
    List<TaskFileResponse> Files
);

/// <summary>Paginated list wrapper.</summary>
public record PagedResponse<T>(
    IEnumerable<T> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
);