// Api/Controllers/UserTaskReportController.cs
using Application.Contracts.UserTaskReport;
using Application.Extensions;
using Application.Services.Reports;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskStatus = Domain.Entities.TaskStatus;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserTaskReportController(IUserTaskReportService reportService) : ControllerBase
{
    // ── GET /api/UserTaskReport/users/{userId}/summary ─────────────────────
    /// <summary>
    /// High-level statistics for a specific user's tasks:
    /// total, done, in-progress, overdue count, average progress, etc.
    /// </summary>
    [HttpGet("users/{userId}/summary")]
    public async Task<IActionResult> GetUserSummary(string userId)
    {
        var result = await reportService.GetUserTaskSummaryAsync(userId);
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }

    // ── GET /api/UserTaskReport/users/{userId}/tasks ───────────────────────
    /// <summary>
    /// Paginated, filtered list of tasks assigned to a specific user.
    ///
    /// Query params:
    ///   isCompleted   bool?       true = Done only | false = non-Done only
    ///   statuses      int[]       filter by one or more TaskStatus values
    ///   priority      int?        TaskPriority enum value
    ///   dueBefore     DateTime?
    ///   dueAfter      DateTime?
    ///   search        string?     searches title + description
    ///   page          int         default 1
    ///   pageSize      int         default 20, max 100
    /// </summary>
    [HttpGet("users/{userId}/tasks")]
    public async Task<IActionResult> GetUserTasks(
        string userId,
        [FromQuery] bool? isCompleted,
        [FromQuery] List<TaskStatus>? statuses,
        [FromQuery] TaskPriority? priority,
        [FromQuery] DateTime? dueBefore,
        [FromQuery] DateTime? dueAfter,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var filter = new UserTaskFilterRequest(
            isCompleted, statuses, priority,
            dueBefore, dueAfter, search,
            page, pageSize);

        var result = await reportService.GetUserTasksAsync(userId, filter);
        return Ok(result);
    }

    // ── GET /api/UserTaskReport/tasks/{taskId} ─────────────────────────────
    /// <summary>
    /// Full detail for a single task: all fields, every assignee with who
    /// assigned them, and occurrence list if recurring.
    /// </summary>
    [HttpGet("tasks/{taskId:int}")]
    public async Task<IActionResult> GetTaskDetail(int taskId)
    {
        var result = await reportService.GetTaskDetailAsync(taskId);
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }

    // ── GET /api/UserTaskReport/daily ──────────────────────────────────────
    /// <summary>
    /// All tasks (and recurring-task occurrences) due on a given date.
    /// Defaults to today if <c>date</c> is not supplied.
    ///
    /// Query params:
    ///   date      DateTime?    defaults to today
    ///   status    int?         optional TaskStatus filter
    ///   page      int          default 1
    ///   pageSize  int          default 20, max 100
    /// </summary>
    [HttpGet("daily")]
    public async Task<IActionResult> GetDailyTasks(
        [FromQuery] DateTime? date,
        [FromQuery] TaskStatus? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var filter = new DailyTaskFilterRequest(date, status, page, pageSize);
        var result = await reportService.GetDailyTasksAsync(filter);
        return Ok(result);
    }
}