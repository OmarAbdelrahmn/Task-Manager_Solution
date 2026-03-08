// Application/Services/UserTaskReport/UserTaskReportService.cs
using Application.Abstraction;
using Application.Abstraction.Errors;
using Application.Contracts.Tasks;
using Application.Contracts.UserTaskReport;
using Domain;
using Domain.Entities;
using Domain.Entities.Identity;
using Domain.Entities.Main;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaskStatus = Domain.Entities.TaskStatus;

namespace Application.Services.Reports;

public class UserTaskReportService(
    ApplicationDbcontext db,
    UserManager<ApplicationUser> userManager
) : IUserTaskReportService
{
    // ═══════════════════════════════════════════════════════════════════════
    //  GET USER TASK SUMMARY
    // ═══════════════════════════════════════════════════════════════════════

    public async Task<Result<UserTaskSummaryResponse>> GetUserTaskSummaryAsync(string targetUserId)
    {
        // Verify the target user exists
        var user = await userManager.FindByIdAsync(targetUserId);
        if (user is null)
            return Result.Failure<UserTaskSummaryResponse>(UserErrors.UserNotFound);

        // Load all non-deleted tasks for this user in one query
        var tasks = await db.Tasks
            .Where(t => !t.IsDeleted &&
                        t.Assignees.Any(a => a.UserId == targetUserId))
            .AsNoTracking()
            .ToListAsync();

        var today = DateTime.Today;

        var summary = new UserTaskSummaryResponse(
            UserId: targetUserId,
            UserName: user.UserName ?? "",
            FullName: user.FullName ?? "",
            AvatarUrl: user.AvatarUrl,

            TotalTasks: tasks.Count,
            TodoCount: tasks.Count(t => t.Status == TaskStatus.Todo),
            InProgressCount: tasks.Count(t => t.Status == TaskStatus.InProgress),
            DoneCount: tasks.Count(t => t.Status == TaskStatus.Done),
            ArchivedCount: tasks.Count(t => t.Status == TaskStatus.Archived),

            // Overdue = not Done or Archived AND past their due date
            OverdueTasks: tasks.Count(t =>
                t.DueDate.HasValue &&
                t.DueDate.Value.Date < today &&
                t.Status != TaskStatus.Done &&
                t.Status != TaskStatus.Archived),

            AverageProgress: tasks
                .Where(t => t.Status != TaskStatus.Archived)
                .Select(t => (double)t.Progress)
                .DefaultIfEmpty(0)
                .Average(),

            RecurringTaskCount: tasks.Count(t => t.IsRecurring)
        );

        return Result.Success(summary);
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  GET USER TASKS  (paginated + filtered)
    // ═══════════════════════════════════════════════════════════════════════

    public async Task<PagedResponse<UserTaskDetailResponse>> GetUserTasksAsync(
        string targetUserId, UserTaskFilterRequest filter)
    {
        // Verify user exists — return empty page instead of 404 to keep it
        // consistent with other list endpoints
        if (await userManager.FindByIdAsync(targetUserId) is null)
            return EmptyPage<UserTaskDetailResponse>(filter.Page, filter.PageSize);

        var query = db.Tasks
            .Where(t => !t.IsDeleted &&
                        t.Assignees.Any(a => a.UserId == targetUserId))
            .AsNoTracking()
            .AsQueryable();

        // ── Optional filters ────────────────────────────────────────────────

        // Completion shorthand (overrides Statuses if both supplied)
        if (filter.IsCompleted.HasValue)
        {
            query = filter.IsCompleted.Value
                ? query.Where(t => t.Status == TaskStatus.Done)
                : query.Where(t => t.Status != TaskStatus.Done);
        }
        else if (filter.Statuses is { Count: > 0 })
        {
            query = query.Where(t => filter.Statuses.Contains(t.Status));
        }

        if (filter.Priority.HasValue)
            query = query.Where(t => t.Priority == filter.Priority.Value);

        if (filter.DueBefore.HasValue)
            query = query.Where(t => t.DueDate.HasValue && t.DueDate <= filter.DueBefore.Value);

        if (filter.DueAfter.HasValue)
            query = query.Where(t => t.DueDate.HasValue && t.DueDate >= filter.DueAfter.Value);

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var term = filter.Search.Trim().ToLower();
            query = query.Where(t =>
                t.Title.ToLower().Contains(term) ||
                (t.Description != null && t.Description.ToLower().Contains(term)));
        }

        // ── Count before paging ─────────────────────────────────────────────
        var total = await query.CountAsync();

        var pageSize = Math.Clamp(filter.PageSize, 1, 100);
        var page = Math.Max(filter.Page, 1);

        // ── Fetch with all needed nav props ─────────────────────────────────
        var tasks = await query
            .Include(t => t.Assignees)
                .ThenInclude(a => a.User)
            .Include(t => t.Assignees)
                .ThenInclude(a => a.AssignedBy)
            .Include(t => t.Occurrences)
            .Include(t => t.Conversation)
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResponse<UserTaskDetailResponse>(
            tasks.Select(MapToUserTaskDetail).ToList(),
            total, page, pageSize,
            (int)Math.Ceiling((double)total / pageSize));
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  GET SINGLE TASK DETAIL
    // ═══════════════════════════════════════════════════════════════════════

    public async Task<Result<UserTaskDetailResponse>> GetTaskDetailAsync(int taskId)
    {
        var task = await db.Tasks
            .Include(t => t.Assignees)
                .ThenInclude(a => a.User)
            .Include(t => t.Assignees)
                .ThenInclude(a => a.AssignedBy)
            .Include(t => t.Occurrences)
            .Include(t => t.Conversation)
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == taskId && !t.IsDeleted);

        if (task is null)
            return Result.Failure<UserTaskDetailResponse>(TaskErrors.TaskNotFound);

        return Result.Success(MapToUserTaskDetail(task));
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  GET DAILY TASKS
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Returns every task and recurring-occurrence due on the given date.
    /// Two separate DB hits are merged and de-duplicated so a recurring task
    /// only appears once even if it has an occurrence on that date AND its own
    /// DueDate matches.
    /// </summary>
    public async Task<PagedResponse<DailyTaskResponse>> GetDailyTasksAsync(
        DailyTaskFilterRequest filter)
    {
        var targetDate = (filter.Date ?? DateTime.Today).Date;
        var nextDay = targetDate.AddDays(1);

        // ── 1. Regular tasks with DueDate on that day ───────────────────────
        var regularQuery = db.Tasks
            .Where(t => !t.IsDeleted &&
                        t.DueDate.HasValue &&
                        t.DueDate.Value >= targetDate &&
                        t.DueDate.Value < nextDay)
            .AsNoTracking()
            .AsQueryable();

        // ── 2. Recurring tasks that have an occurrence on that day ──────────
        var occurrenceTaskIds = await db.TaskOccurrences
            .Where(o => o.DueDate >= targetDate && o.DueDate < nextDay)
            .Select(o => o.TaskId)
            .Distinct()
            .ToListAsync();

        var recurringQuery = db.Tasks
            .Where(t => !t.IsDeleted && occurrenceTaskIds.Contains(t.Id))
            .AsNoTracking()
            .AsQueryable();

        // Merge and de-duplicate by task ID
        var regularIds = await regularQuery.Select(t => t.Id).ToListAsync();
        var recurringIds = await recurringQuery.Select(t => t.Id).ToListAsync();
        var allIds = regularIds.Union(recurringIds).Distinct().ToList();

        // ── Optional status filter ──────────────────────────────────────────
        var baseQuery = db.Tasks
            .Where(t => allIds.Contains(t.Id))
            .AsNoTracking()
            .AsQueryable();

        if (filter.Status.HasValue)
            baseQuery = baseQuery.Where(t => t.Status == filter.Status.Value);

        // ── Count + page ────────────────────────────────────────────────────
        var total = await baseQuery.CountAsync();
        var pageSize = Math.Clamp(filter.PageSize, 1, 100);
        var page = Math.Max(filter.Page, 1);

        var tasks = await baseQuery
            .Include(t => t.Assignees)
                .ThenInclude(a => a.User)
            .Include(t => t.Assignees)
                .ThenInclude(a => a.AssignedBy)
            .OrderBy(t => t.DueDate)
            .ThenBy(t => t.Priority)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var today = DateTime.Today;
        var items = tasks.Select(t => new DailyTaskResponse(
            TaskId: t.Id,
            Title: t.Title,
            Description: t.Description,
            Status: t.Status,
            Priority: t.Priority,
            Progress: t.Progress,
            DueDate: t.DueDate,
            IsOverdue: t.DueDate.HasValue &&
                         t.DueDate.Value.Date < today &&
                         t.Status != TaskStatus.Done &&
                         t.Status != TaskStatus.Archived,
            IsRecurring: t.IsRecurring,
            Assignees: t.Assignees.Select(MapToAssigneeDetail).ToList()
        )).ToList();

        return new PagedResponse<DailyTaskResponse>(
            items, total, page, pageSize,
            (int)Math.Ceiling((double)total / pageSize));
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  PRIVATE MAPPERS
    // ═══════════════════════════════════════════════════════════════════════

    private static UserTaskDetailResponse MapToUserTaskDetail(AppTask t)
    {
        var today = DateTime.Today;
        return new UserTaskDetailResponse(
            Id: t.Id,
            Title: t.Title,
            Description: t.Description,
            Status: t.Status,
            Priority: t.Priority,
            Progress: t.Progress,
            DueDate: t.DueDate,
            IsOverdue: t.DueDate.HasValue &&
                                 t.DueDate.Value.Date < today &&
                                 t.Status != TaskStatus.Done &&
                                 t.Status != TaskStatus.Archived,
            IsRecurring: t.IsRecurring,
            RecurrenceType: t.RecurrenceType,
            RecurrenceInterval: t.RecurrenceInterval,
            RecurrenceStartDate: t.RecurrenceStartDate,
            RecurrenceEndDate: t.RecurrenceEndDate,
            CreatedAt: t.CreatedAt,
            UpdatedAt: t.UpdatedAt,
            CreatedById: t.CreatedById,
            Assignees: t.Assignees.Select(MapToAssigneeDetail).ToList(),
            Occurrences: t.Occurrences
                                  .OrderBy(o => o.DueDate)
                                  .Select(MapToOccurrenceBrief)
                                  .ToList(),
            ConversationId: t.Conversation?.Id
        );
    }

    private static AssigneeDetailResponse MapToAssigneeDetail(TaskAssignee a) =>
        new(
            UserId: a.UserId,
            UserName: a.User?.UserName ?? "",
            FullName: a.User?.FullName ?? "",
            AvatarUrl: a.User?.AvatarUrl,

            AssignedById: a.AssignedById,
            AssignedByUserName: a.AssignedBy?.UserName ?? "",
            AssignedByFullName: a.AssignedBy?.FullName ?? "",
            AssignedAt: a.AssignedAt
        );

    private static TaskOccurrenceBriefResponse MapToOccurrenceBrief(TaskOccurrence o) =>
        new(
            Id: o.Id,
            DueDate: o.DueDate,
            Status: o.Status,
            Progress: o.Progress,
            Notes: o.Notes,
            CompletedAt: o.CompletedAt
        );

    // ═══════════════════════════════════════════════════════════════════════
    //  PRIVATE HELPERS
    // ═══════════════════════════════════════════════════════════════════════

    private static PagedResponse<T> EmptyPage<T>(int page, int pageSize) =>
        new([], 0, page, pageSize, 0);
}