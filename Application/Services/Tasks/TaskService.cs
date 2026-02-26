// Application/Services/Tasks/TaskService.cs
using Application.Abstraction;
using Application.Abstraction.Errors;
using Application.Contracts.Tasks;
using Domain;
using Domain.Entities;
using Domain.Entities.Identity;
using Domain.Entities.Main;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.Tasks;

public class TaskService(
    ApplicationDbcontext db,
    UserManager<ApplicationUser> userManager,
    IWebHostEnvironment env         // used for physical file storage
) : ITaskService
{
    // ═══════════════════════════════════════════════════════════════════════
    //  QUERIES
    // ═══════════════════════════════════════════════════════════════════════

    public async Task<PagedResponse<TaskSummaryResponse>> GetAllTasksAsync(TaskFilterRequest filter)
    {
        // Build the base query
        var query = db.Tasks
            .Include(t => t.Assignees)
                .ThenInclude(a => a.User)
            .AsNoTracking()
            .AsQueryable();

        // ── Soft-delete filter ──────────────────────────────────────────────
        query = filter.IncludeDeleted
            ? query
            : query.Where(t => !t.IsDeleted);

        // ── Text search ─────────────────────────────────────────────────────
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var term = filter.Search.ToLower().Trim();
            query = query.Where(t =>
                t.Title.ToLower().Contains(term) ||
                (t.Description != null && t.Description.ToLower().Contains(term)));
        }

        // ── Status ──────────────────────────────────────────────────────────
        if (filter.Status.HasValue)
            query = query.Where(t => t.Status == filter.Status.Value);

        // ── Priority ────────────────────────────────────────────────────────
        if (filter.Priority.HasValue)
            query = query.Where(t => t.Priority == filter.Priority.Value);

        // ── Assignee ────────────────────────────────────────────────────────
        if (!string.IsNullOrWhiteSpace(filter.AssignedUserId))
            query = query.Where(t => t.Assignees.Any(a => a.UserId == filter.AssignedUserId));

        // ── Recurrence ──────────────────────────────────────────────────────
        if (filter.IsRecurring.HasValue)
            query = query.Where(t => t.IsRecurring == filter.IsRecurring.Value);

        // ── Due-date window ─────────────────────────────────────────────────
        if (filter.DueBefore.HasValue)
            query = query.Where(t => t.DueDate.HasValue && t.DueDate <= filter.DueBefore.Value);

        if (filter.DueAfter.HasValue)
            query = query.Where(t => t.DueDate.HasValue && t.DueDate >= filter.DueAfter.Value);

        // ── Count before paging ─────────────────────────────────────────────
        var totalCount = await query.CountAsync();

        // ── Ordering + paging ───────────────────────────────────────────────
        var pageSize = Math.Clamp(filter.PageSize, 1, 100);
        var page = Math.Max(filter.Page, 1);

        var tasks = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var items = tasks.Select(MapToSummary).ToList();

        return new PagedResponse<TaskSummaryResponse>(
            items,
            totalCount,
            page,
            pageSize,
            (int)Math.Ceiling((double)totalCount / pageSize)
        );
    }

    public async Task<Result<TaskDetailResponse>> GetTaskByIdAsync(int taskId)
    {
        var task = await LoadTaskDetailAsync(taskId);

        if (task is null)
            return Result.Failure<TaskDetailResponse>(TaskErrors.TaskNotFound);

        return Result.Success(MapToDetail(task));
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  COMMANDS — Core
    // ═══════════════════════════════════════════════════════════════════════

    public async Task<Result<TaskDetailResponse>> CreateTaskAsync(
        CreateTaskRequest request, string createdById)
    {
        // ── Validate recurrence fields ──────────────────────────────────────
        if (request.IsRecurring &&
            (request.RecurrenceType is null || request.RecurrenceInterval is null))
            return Result.Failure<TaskDetailResponse>(TaskErrors.InvalidRecurrence);

        // ── Build entity ────────────────────────────────────────────────────
        var task = new AppTask
        {
            Title = request.Title.Trim(),
            Description = request.Description?.Trim(),
            Priority = request.Priority,
            Status = TaskStatus.Todo,
            Progress = 0,
            DueDate = request.DueDate,
            IsRecurring = request.IsRecurring,
            RecurrenceType = request.IsRecurring ? request.RecurrenceType : null,
            RecurrenceInterval = request.IsRecurring ? request.RecurrenceInterval : null,
            RecurrenceStartDate = request.IsRecurring ? request.RecurrenceStartDate : null,
            RecurrenceEndDate = request.IsRecurring ? request.RecurrenceEndDate : null,
            CreatedById = createdById
        };

        db.Tasks.Add(task);
        await db.SaveChangesAsync();

        // ── Assign initial users ────────────────────────────────────────────
        if (request.AssigneeIds is { Count: > 0 })
        {
            foreach (var userId in request.AssigneeIds.Distinct())
            {
                if (await userManager.FindByIdAsync(userId) is not null)
                {
                    db.TaskAssignees.Add(new TaskAssignee
                    {
                        TaskId = task.Id,
                        UserId = userId,
                        AssignedById = createdById
                    });
                }
            }
            await db.SaveChangesAsync();
        }

        // ── Auto-generate first occurrence if recurring ─────────────────────
        if (task.IsRecurring)
        {
            var firstDue = task.RecurrenceStartDate ?? task.DueDate ?? DateTime.Today;
            db.TaskOccurrences.Add(new TaskOccurrence
            {
                TaskId = task.Id,
                DueDate = firstDue,
                Status = TaskStatus.Todo
            });
            await db.SaveChangesAsync();
        }

        // ── Auto-create a TaskThread conversation ───────────────────────────
        var conversation = new Conversation
        {
            Type = ConversationType.TaskThread,
            TaskId = task.Id,
            CreatedById = createdById
        };
        db.Conversations.Add(conversation);
        await db.SaveChangesAsync();

        var created = await LoadTaskDetailAsync(task.Id);
        return Result.Success(MapToDetail(created!));
    }

    public async Task<Result<TaskDetailResponse>> UpdateTaskAsync(
        int taskId, UpdateTaskRequest request, string updatedById)
    {
        var task = await db.Tasks.FindAsync(taskId);

        if (task is null || task.IsDeleted)
            return Result.Failure<TaskDetailResponse>(TaskErrors.TaskNotFound);

        if (task.Status == TaskStatus.Archived)
            return Result.Failure<TaskDetailResponse>(TaskErrors.TaskAlreadyArchived);

        // ── Validate recurrence ─────────────────────────────────────────────
        if (request.IsRecurring &&
            (request.RecurrenceType is null || request.RecurrenceInterval is null))
            return Result.Failure<TaskDetailResponse>(TaskErrors.InvalidRecurrence);

        task.Title = request.Title.Trim();
        task.Description = request.Description?.Trim();
        task.Priority = request.Priority;
        task.DueDate = request.DueDate;
        task.IsRecurring = request.IsRecurring;
        task.RecurrenceType = request.IsRecurring ? request.RecurrenceType : null;
        task.RecurrenceInterval = request.IsRecurring ? request.RecurrenceInterval : null;
        task.RecurrenceStartDate = request.IsRecurring ? request.RecurrenceStartDate : null;
        task.RecurrenceEndDate = request.IsRecurring ? request.RecurrenceEndDate : null;
        task.UpdatedAt = DateTime.Now;
        task.UpdatedById = updatedById;

        await db.SaveChangesAsync();

        var updated = await LoadTaskDetailAsync(taskId);
        return Result.Success(MapToDetail(updated!));
    }

    public async Task<Result<TaskDetailResponse>> UpdateProgressAsync(
        int taskId, UpdateProgressRequest request, string updatedById)
    {
        if (request.Progress < 0 || request.Progress > 100)
            return Result.Failure<TaskDetailResponse>(TaskErrors.InvalidProgress);

        var task = await db.Tasks.FindAsync(taskId);

        if (task is null || task.IsDeleted)
            return Result.Failure<TaskDetailResponse>(TaskErrors.TaskNotFound);

        if (task.Status == TaskStatus.Archived)
            return Result.Failure<TaskDetailResponse>(TaskErrors.TaskAlreadyArchived);

        task.Progress = request.Progress;
        task.UpdatedAt = DateTime.Now;
        task.UpdatedById = updatedById;

        // Recalculate status from progress
        task.RecalculateStatus(task);

        await db.SaveChangesAsync();

        var updated = await LoadTaskDetailAsync(taskId);
        return Result.Success(MapToDetail(updated!));
    }

    public async Task<Result> DeleteTaskAsync(int taskId, string deletedById)
    {
        var task = await db.Tasks.FindAsync(taskId);

        if (task is null || task.IsDeleted)
            return Result.Failure(TaskErrors.TaskNotFound);

        if (task.Status == TaskStatus.Archived)
            return Result.Failure(TaskErrors.CannotDeleteArchivedTask);

        task.IsDeleted = true;
        task.DeletedAt = DateTime.Now;
        task.DeletedById = deletedById;

        await db.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result> ArchiveTaskAsync(int taskId, string updatedById)
    {
        var task = await db.Tasks.FindAsync(taskId);

        if (task is null || task.IsDeleted)
            return Result.Failure(TaskErrors.TaskNotFound);

        if (task.Status == TaskStatus.Archived)
            return Result.Failure(TaskErrors.TaskAlreadyArchived);

        task.Status = TaskStatus.Archived;
        task.UpdatedAt = DateTime.Now;
        task.UpdatedById = updatedById;

        await db.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result> RestoreTaskAsync(int taskId, string updatedById)
    {
        var task = await db.Tasks
            .IgnoreQueryFilters()           // in case global filters hide deleted
            .FirstOrDefaultAsync(t => t.Id == taskId);

        if (task is null)
            return Result.Failure(TaskErrors.TaskNotFound);

        if (!task.IsDeleted)
            return Result.Failure(TaskErrors.TaskNotFound);

        task.IsDeleted = false;
        task.DeletedAt = null;
        task.DeletedById = null;
        task.UpdatedAt = DateTime.Now;
        task.UpdatedById = updatedById;

        // Restore status from progress
        task.RecalculateStatus(task);

        await db.SaveChangesAsync();
        return Result.Success();
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  COMMANDS — Assignees
    // ═══════════════════════════════════════════════════════════════════════

    public async Task<Result> AssignUserAsync(int taskId, string userId, string assignedById)
    {
        var task = await db.Tasks.FindAsync(taskId);

        if (task is null || task.IsDeleted)
            return Result.Failure(TaskErrors.TaskNotFound);

        if (await userManager.FindByIdAsync(userId) is null)
            return Result.Failure(UserErrors.UserNotFound);

        var alreadyAssigned = await db.TaskAssignees
            .AnyAsync(a => a.TaskId == taskId && a.UserId == userId);

        if (alreadyAssigned)
            return Result.Failure(TaskErrors.AssigneeAlreadyExists);

        db.TaskAssignees.Add(new TaskAssignee
        {
            TaskId = taskId,
            UserId = userId,
            AssignedById = assignedById
        });

        await db.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result> UnassignUserAsync(int taskId, string userId)
    {
        var assignee = await db.TaskAssignees
            .FirstOrDefaultAsync(a => a.TaskId == taskId && a.UserId == userId);

        if (assignee is null)
            return Result.Failure(TaskErrors.AssigneeNotFound);

        db.TaskAssignees.Remove(assignee);
        await db.SaveChangesAsync();
        return Result.Success();
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  COMMANDS — Files
    // ═══════════════════════════════════════════════════════════════════════

    public async Task<Result<TaskFileResponse>> UploadFileAsync(
        int taskId, IFormFile file, string uploadedById, int? occurrenceId = null)
    {
        var task = await db.Tasks.FindAsync(taskId);

        if (task is null || task.IsDeleted)
            return Result.Failure<TaskFileResponse>(TaskErrors.TaskNotFound);

        // ── Validate occurrence belongs to this task ────────────────────────
        if (occurrenceId.HasValue)
        {
            var occurrenceExists = await db.TaskOccurrences
                .AnyAsync(o => o.Id == occurrenceId.Value && o.TaskId == taskId);

            if (!occurrenceExists)
                return Result.Failure<TaskFileResponse>(TaskErrors.OccurrenceNotFound);
        }

        // ── Persist file to disk ────────────────────────────────────────────
        var storedFileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var uploadFolder = Path.Combine(env.WebRootPath, "uploads", "tasks", taskId.ToString());
        Directory.CreateDirectory(uploadFolder);

        var physicalPath = Path.Combine(uploadFolder, storedFileName);
        await using (var stream = new FileStream(physicalPath, FileMode.Create))
            await file.CopyToAsync(stream);

        var fileUrl = $"/uploads/tasks/{taskId}/{storedFileName}";

        // ── Save record ─────────────────────────────────────────────────────
        var taskFile = new TaskFile
        {
            TaskId = taskId,
            OccurrenceId = occurrenceId,
            FileName = file.FileName,
            StoredFileName = storedFileName,
            FileUrl = fileUrl,
            MimeType = file.ContentType,
            FileSize = file.Length,
            CreatedById = uploadedById
        };

        db.TaskFiles.Add(taskFile);
        await db.SaveChangesAsync();

        return Result.Success(MapToFileResponse(taskFile));
    }

    public async Task<Result> DeleteFileAsync(int taskId, int fileId)
    {
        var file = await db.TaskFiles
            .FirstOrDefaultAsync(f => f.Id == fileId && f.TaskId == taskId);

        if (file is null)
            return Result.Failure(TaskErrors.FileNotFound);

        // ── Remove physical file ────────────────────────────────────────────
        var physicalPath = Path.Combine(
            env.WebRootPath, "uploads", "tasks",
            taskId.ToString(), file.StoredFileName);

        if (File.Exists(physicalPath))
            File.Delete(physicalPath);

        db.TaskFiles.Remove(file);
        await db.SaveChangesAsync();
        return Result.Success();
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  COMMANDS — Occurrences
    // ═══════════════════════════════════════════════════════════════════════

    public async Task<Result<IEnumerable<TaskOccurrenceResponse>>> GetOccurrencesAsync(int taskId)
    {
        var task = await db.Tasks.FindAsync(taskId);

        if (task is null || task.IsDeleted)
            return Result.Failure<IEnumerable<TaskOccurrenceResponse>>(TaskErrors.TaskNotFound);

        if (!task.IsRecurring)
            return Result.Failure<IEnumerable<TaskOccurrenceResponse>>(TaskErrors.NonRecurringTask);

        var occurrences = await db.TaskOccurrences
            .Include(o => o.Files)
            .Where(o => o.TaskId == taskId)
            .OrderBy(o => o.DueDate)
            .AsNoTracking()
            .ToListAsync();

        return Result.Success(occurrences.Select(MapToOccurrenceResponse));
    }

    public async Task<Result<TaskOccurrenceResponse>> UpdateOccurrenceAsync(
        int taskId, int occurrenceId, UpdateOccurrenceRequest request, string updatedById)
    {
        if (request.Progress < 0 || request.Progress > 100)
            return Result.Failure<TaskOccurrenceResponse>(TaskErrors.InvalidProgress);

        var occurrence = await db.TaskOccurrences
            .Include(o => o.Files)
            .FirstOrDefaultAsync(o => o.Id == occurrenceId && o.TaskId == taskId);

        if (occurrence is null)
            return Result.Failure<TaskOccurrenceResponse>(TaskErrors.OccurrenceNotFound);

        occurrence.Progress = request.Progress;
        occurrence.Notes = request.Notes;
        occurrence.RecalculateStatus();   // auto-updates Status + CompletedAt

        await db.SaveChangesAsync();

        // ── Bubble up: if all occurrences done → mark parent task Done ──────
        await SyncParentTaskStatusAsync(taskId, updatedById);

        return Result.Success(MapToOccurrenceResponse(occurrence));
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  PRIVATE HELPERS
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// If every non-archived occurrence is Done, set parent task → Done (100 %).
    /// If at least one occurrence is InProgress, set parent task → InProgress.
    /// Otherwise leave as Todo.
    /// </summary>
    private async Task SyncParentTaskStatusAsync(int taskId, string updatedById)
    {
        var task = await db.Tasks.FindAsync(taskId);
        if (task is null || task.Status == TaskStatus.Archived) return;

        var statuses = await db.TaskOccurrences
            .Where(o => o.TaskId == taskId)
            .Select(o => o.Status)
            .ToListAsync();

        if (statuses.Count == 0) return;

        bool allDone = statuses.All(s => s == TaskStatus.Done);
        bool anyInProgress = statuses.Any(s => s == TaskStatus.InProgress);

        task.Status = allDone ? TaskStatus.Done
                         : anyInProgress ? TaskStatus.InProgress
                         : TaskStatus.Todo;
        task.Progress = allDone ? 100
                         : (int)statuses.Average(s =>
                             s == TaskStatus.Done ? 100 :
                             s == TaskStatus.InProgress ? 50 : 0);
        task.UpdatedAt = DateTime.Now;
        task.UpdatedById = updatedById;

        await db.SaveChangesAsync();
    }

    /// <summary>Eager-load a single task with all navigation props needed for detail view.</summary>
    private async Task<AppTask?> LoadTaskDetailAsync(int taskId)
    {
        return await db.Tasks
            .Include(t => t.Assignees)
                .ThenInclude(a => a.User)
            .Include(t => t.Assignees)
                .ThenInclude(a => a.AssignedBy)
            .Include(t => t.Files)
            .Include(t => t.Occurrences)
                .ThenInclude(o => o.Files)
            .Include(t => t.Conversation)
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == taskId);
    }

    // ─────────────────────────────────────────────
    //  Mappers
    // ─────────────────────────────────────────────

    private static TaskSummaryResponse MapToSummary(AppTask t) => new(
        t.Id,
        t.Title,
        t.Status,
        t.Priority,
        t.Progress,
        t.DueDate,
        t.IsRecurring,
        t.CreatedAt,
        t.CreatedById,
        t.IsDeleted,
        t.Assignees.Select(MapToAssigneeSummary).ToList()
    );

    private static TaskDetailResponse MapToDetail(AppTask t) => new(
        t.Id,
        t.Title,
        t.Description,
        t.Status,
        t.Priority,
        t.Progress,
        t.DueDate,
        t.IsRecurring,
        t.RecurrenceType,
        t.RecurrenceInterval,
        t.RecurrenceStartDate,
        t.RecurrenceEndDate,
        t.CreatedAt,
        t.UpdatedAt,
        t.CreatedById,
        t.UpdatedById,
        t.IsDeleted,
        t.DeletedAt,
        t.Assignees.Select(MapToAssigneeSummary).ToList(),
        t.Files.Select(MapToFileResponse).ToList(),
        t.Occurrences.OrderBy(o => o.DueDate).Select(MapToOccurrenceResponse).ToList(),
        t.Conversation?.Id
    );

    private static AssigneeSummary MapToAssigneeSummary(TaskAssignee a) => new(
        a.UserId,
        a.User?.UserName ?? "",
        a.User?.FullName ?? "",
        a.User?.AvatarUrl,
        a.AssignedById,
        a.AssignedAt
    );

    private static TaskFileResponse MapToFileResponse(TaskFile f) => new(
        f.Id,
        f.FileName,
        f.FileUrl,
        f.MimeType,
        f.FileSize,
        f.OccurrenceId,
        f.CreatedAt,
        f.CreatedById
    );

    private static TaskOccurrenceResponse MapToOccurrenceResponse(TaskOccurrence o) => new(
        o.Id,
        o.DueDate,
        o.Status,
        o.Progress,
        o.Notes,
        o.CompletedAt,
        o.Files.Select(MapToFileResponse).ToList()
    );
}