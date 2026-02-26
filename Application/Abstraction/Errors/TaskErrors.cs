// Application/Abstraction/Errors/TaskErrors.cs
using Microsoft.AspNetCore.Http;

namespace Application.Abstraction.Errors;

public static class TaskErrors
{
    public static readonly Error TaskNotFound =
        new("Task.NotFound", "Task was not found.", StatusCodes.Status404NotFound);

    public static readonly Error TaskAlreadyArchived =
        new("Task.AlreadyArchived", "Task is already archived.", StatusCodes.Status400BadRequest);

    public static readonly Error TaskAlreadyDeleted =
        new("Task.AlreadyDeleted", "Task has already been deleted.", StatusCodes.Status400BadRequest);

    public static readonly Error InvalidProgress =
        new("Task.InvalidProgress", "Progress must be between 0 and 100.", StatusCodes.Status400BadRequest);

    public static readonly Error AssigneeAlreadyExists =
        new("Task.AssigneeAlreadyExists", "This user is already assigned to the task.", StatusCodes.Status409Conflict);

    public static readonly Error AssigneeNotFound =
        new("Task.AssigneeNotFound", "This user is not assigned to the task.", StatusCodes.Status404NotFound);

    public static readonly Error FileNotFound =
        new("Task.FileNotFound", "The requested file was not found.", StatusCodes.Status404NotFound);

    public static readonly Error OccurrenceNotFound =
        new("Task.OccurrenceNotFound", "Task occurrence was not found.", StatusCodes.Status404NotFound);

    public static readonly Error NonRecurringTask =
        new("Task.NonRecurring", "This task is not a recurring task.", StatusCodes.Status400BadRequest);

    public static readonly Error InvalidRecurrence =
        new("Task.InvalidRecurrence", "Recurring tasks must have a RecurrenceType and RecurrenceInterval.", StatusCodes.Status400BadRequest);

    public static readonly Error InvalidDueDate =
        new("Task.InvalidDueDate", "Due date cannot be in the past.", StatusCodes.Status400BadRequest);

    public static readonly Error CannotDeleteArchivedTask =
        new("Task.CannotDeleteArchived", "Archived tasks cannot be deleted. Unarchive first.", StatusCodes.Status400BadRequest);
}