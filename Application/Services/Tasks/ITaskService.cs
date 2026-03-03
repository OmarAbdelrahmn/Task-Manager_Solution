// Application/Services/Tasks/ITaskService.cs
using Application.Abstraction;
using Application.Contracts.Tasks;
using Microsoft.AspNetCore.Http;

namespace Application.Services.Tasks;

public interface ITaskService
{
    // ── Queries ─────────────────────────────────────────────────────────────

    Task<Result<IEnumerable<TaskSummaryResponse>>> GetMyTasksAsync(string userId);

    /// <summary>Paginated, filtered list of tasks.</summary>
    Task<PagedResponse<TaskSummaryResponse>> GetAllTasksAsync(TaskFilterRequest filter);

    /// <summary>Full detail for a single task by ID.</summary>
    Task<Result<TaskDetailResponse>> GetTaskByIdAsync(int taskId);

    // ── Commands ─────────────────────────────────────────────────────────────

    /// <summary>Create a task, optionally with initial assignees.</summary>
    Task<Result<TaskDetailResponse>> CreateTaskAsync(CreateTaskRequest request, string createdById);

    /// <summary>Update the editable fields of a task.</summary>
    Task<Result<TaskDetailResponse>> UpdateTaskAsync(int taskId, UpdateTaskRequest request, string updatedById);

    /// <summary>
    /// Update progress (0–100). Status is automatically recalculated:
    /// 0 → Todo | 1-99 → InProgress | 100 → Done
    /// </summary>
    Task<Result<TaskDetailResponse>> UpdateProgressAsync(int taskId, UpdateProgressRequest request, string updatedById);

    /// <summary>Soft-delete a task (sets IsDeleted + DeletedAt).</summary>
    Task<Result> DeleteTaskAsync(int taskId, string deletedById);

    /// <summary>Archive a task (Status = Archived). No hard delete.</summary>
    Task<Result> ArchiveTaskAsync(int taskId, string updatedById);

    /// <summary>Restore a soft-deleted task.</summary>
    Task<Result> RestoreTaskAsync(int taskId, string updatedById);

    // ── Assignees ─────────────────────────────────────────────────────────────

    /// <summary>Assign a user to a task.</summary>
    Task<Result> AssignUserAsync(int taskId, string userId, string assignedById);

    /// <summary>Remove a user from a task.</summary>
    Task<Result> UnassignUserAsync(int taskId, string userId);

    // ── Files ─────────────────────────────────────────────────────────────────

    /// <summary>Upload a file attached to a task (or a specific occurrence).</summary>
    Task<Result<TaskFileResponse>> UploadFileAsync(int taskId, IFormFile file, string uploadedById, int? occurrenceId = null);

    /// <summary>Delete a file from a task.</summary>
    Task<Result> DeleteFileAsync(int taskId, int fileId, string deletedById);

    // ── Occurrences (recurring tasks) ──────────────────────────────────────────

    /// <summary>List all occurrences for a recurring task.</summary>
    Task<Result<IEnumerable<TaskOccurrenceResponse>>> GetOccurrencesAsync(int taskId);

    /// <summary>Update progress/notes on a single occurrence.</summary>
    Task<Result<TaskOccurrenceResponse>> UpdateOccurrenceAsync(int taskId, int occurrenceId, UpdateOccurrenceRequest request, string updatedById);
}