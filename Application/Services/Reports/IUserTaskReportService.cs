// Application/Services/UserTaskReport/IUserTaskReportService.cs
using Application.Abstraction;
using Application.Contracts.Tasks;
using Application.Contracts.UserTaskReport;

namespace Application.Services.Reports;

public interface IUserTaskReportService
{
    // ── Per-user overview ──────────────────────────────────────────────────────

    /// <summary>
    /// Returns aggregate statistics for a given user's tasks
    /// (total, done, in-progress, overdue, average progress …).
    /// </summary>
    Task<Result<UserTaskSummaryResponse>> GetUserTaskSummaryAsync(string targetUserId);

    // ── Per-user task list ─────────────────────────────────────────────────────

    /// <summary>
    /// Returns a paginated, filterable list of tasks that <paramref name="targetUserId"/>
    /// is assigned to — with full assignee + assigner details on every row.
    /// </summary>
    Task<PagedResponse<UserTaskDetailResponse>> GetUserTasksAsync(
        string targetUserId, UserTaskFilterRequest filter);

    // ── Single task ────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns full detail for one task, including every assignee and who
    /// performed each assignment.
    /// </summary>
    Task<Result<UserTaskDetailResponse>> GetTaskDetailAsync(int taskId);

    // ── Daily view ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns every task due on the requested date (defaults to today),
    /// with each task's assignees and their assigner info.
    /// Covers both regular tasks (by DueDate) and recurring-task occurrences.
    /// </summary>
    Task<PagedResponse<DailyTaskResponse>> GetDailyTasksAsync(DailyTaskFilterRequest filter);
}