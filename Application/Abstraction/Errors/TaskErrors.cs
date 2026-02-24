// Application/Abstraction/Errors/TaskErrors.cs
using Microsoft.AspNetCore.Http;

namespace Application.Abstraction.Errors;

public static class TaskErrors
{
    public static readonly Error NotFound =
        new("Task.NotFound", "Task not found", StatusCodes.Status404NotFound);

    public static readonly Error Forbidden =
        new("Task.Forbidden", "You don't have access to this task", StatusCodes.Status403Forbidden);

    public static readonly Error InvalidProgress =
        new("Task.InvalidProgress", "Progress must be between 0 and 100", StatusCodes.Status400BadRequest);

    public static readonly Error AlreadyAssigned =
        new("Task.AlreadyAssigned", "This user is already assigned to the task", StatusCodes.Status409Conflict);

    public static readonly Error AssigneeNotFound =
        new("Task.AssigneeNotFound", "Assignee not found on this task", StatusCodes.Status404NotFound);

    public static readonly Error OccurrenceNotFound =
        new("Task.OccurrenceNotFound", "Task occurrence not found", StatusCodes.Status404NotFound);

    public static readonly Error CannotEditRecurring =
        new("Task.CannotEditRecurring", "Cannot edit occurrence of a non-recurring task", StatusCodes.Status400BadRequest);
}