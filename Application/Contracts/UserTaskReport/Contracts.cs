// Application/Contracts/UserTaskReport/UserTaskReportContracts.cs
using Domain.Entities;
using TaskStatus = Domain.Entities.TaskStatus;

namespace Application.Contracts.UserTaskReport;

// ─── Filter / Query ────────────────────────────────────────────────────────────

/// <summary>
/// Query parameters for browsing a specific user's tasks.
/// All filters are optional; omit them to get everything.
/// </summary>
public record UserTaskFilterRequest(
    /// <summary>Filter by completion: true = Done only, false = not-Done only, null = all</summary>
    bool? IsCompleted,

    /// <summary>Filter by one or more statuses (comma-separated in JSON as array)</summary>
    List<TaskStatus>? Statuses,

    /// <summary>Filter by priority</summary>
    TaskPriority? Priority,

    /// <summary>Filter by due date range</summary>
    DateTime? DueBefore,
    DateTime? DueAfter,

    /// <summary>Free-text search against title / description</summary>
    string? Search,

    int Page = 1,
    int PageSize = 20
);

/// <summary>
/// Query parameters for the daily-task view.
/// Defaults to today if Date is not supplied.
/// </summary>
public record DailyTaskFilterRequest(
    DateTime? Date,            // null → today
    TaskStatus? Status,        // optional status narrowing
    int Page = 1,
    int PageSize = 20
);

// ─── Responses ─────────────────────────────────────────────────────────────────

/// <summary>High-level statistics for a single user's task workload.</summary>
public record UserTaskSummaryResponse(
    string UserId,
    string UserName,
    string FullName,
    string? AvatarUrl,

    int TotalTasks,
    int TodoCount,
    int InProgressCount,
    int DoneCount,
    int ArchivedCount,

    int OverdueTasks,        // not Done AND DueDate < today
    double AverageProgress,     // average Progress across non-archived tasks
    int RecurringTaskCount
);

/// <summary>
/// Detailed task row used in list/report views.
/// Includes both who is assigned to the task and who performed each assignment.
/// </summary>
public record UserTaskDetailResponse(
    int Id,
    string Title,
    string? Description,
    TaskStatus Status,
    TaskPriority Priority,
    int Progress,
    DateTime? DueDate,
    bool IsOverdue,
    bool IsRecurring,
    RecurrenceType? RecurrenceType,
    int? RecurrenceInterval,
    DateTime? RecurrenceStartDate,
    DateTime? RecurrenceEndDate,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    string? CreatedById,

    /// <summary>Every person currently assigned to this task.</summary>
    List<AssigneeDetailResponse> Assignees,

    /// <summary>Occurrences (only populated for recurring tasks).</summary>
    List<TaskOccurrenceBriefResponse> Occurrences,

    /// <summary>Id of the linked TaskThread conversation (if any).</summary>
    int? ConversationId
);

/// <summary>
/// Richer assignee record: includes who did the assigning.
/// </summary>
public record AssigneeDetailResponse(
    string UserId,
    string UserName,
    string FullName,
    string? AvatarUrl,

    /// <summary>The person who added this user to the task.</summary>
    string? AssignedById,
    string? AssignedByUserName,
    string? AssignedByFullName,
    DateTime AssignedAt
);

/// <summary>Brief occurrence row used inside a task detail.</summary>
public record TaskOccurrenceBriefResponse(
    int Id,
    DateTime DueDate,
    TaskStatus Status,
    int Progress,
    string? Notes,
    DateTime? CompletedAt
);

// ─── Daily-task response ───────────────────────────────────────────────────────

/// <summary>
/// A single entry in the daily-task report.
/// Shows the task, who is assigned to it, and who made each assignment.
/// </summary>
public record DailyTaskResponse(
    int TaskId,
    string Title,
    string? Description,
    TaskStatus Status,
    TaskPriority Priority,
    int Progress,
    DateTime? DueDate,
    bool IsOverdue,
    bool IsRecurring,

    List<AssigneeDetailResponse> Assignees
);