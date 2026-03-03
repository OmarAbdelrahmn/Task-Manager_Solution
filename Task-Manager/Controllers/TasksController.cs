// Task-Manager/Controllers/TasksController.cs
using Application.Contracts.Tasks;
using Application.Extensions;
using Application.Services.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Task_Manager.Controllers;

[Route("api/tasks")]
[ApiController]
[Authorize]
public class TasksController(ITaskService service) : ControllerBase
{


    [HttpGet("my")]
    public async Task<IActionResult> GetMyTasks()
    {
        var result = await service.GetMyTasksAsync(User.GetUserId()!);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }


    // GET api/tasks
    [HttpGet("")]
    public async Task<IActionResult> GetAll([FromQuery] TaskFilterRequest filter)
    {
        var result = await service.GetAllTasksAsync(filter);
        return Ok(result);
    }

    // GET api/tasks/{id}
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await service.GetTaskByIdAsync(id);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    // POST api/tasks
    [HttpPost("")]
    public async Task<IActionResult> Create([FromBody] CreateTaskRequest request)
    {
        var result = await service.CreateTaskAsync(request, User.GetUserId()!);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value)
            : result.ToProblem();
    }

    // PUT api/tasks/{id}
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateTaskRequest request)
    {
        var result = await service.UpdateTaskAsync(id, request, User.GetUserId()!);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    // PATCH api/tasks/{id}/progress
    [HttpPatch("{id:int}/progress")]
    public async Task<IActionResult> UpdateProgress(int id, [FromBody] UpdateProgressRequest request)
    {
        var result = await service.UpdateProgressAsync(id, request, User.GetUserId()!);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    // DELETE api/tasks/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await service.DeleteTaskAsync(id, User.GetUserId()!);
        return result.IsSuccess ? Ok(new { message = "Task deleted successfully" }) : result.ToProblem();
    }

    // PUT api/tasks/{id}/archive
    [HttpPut("{id:int}/archive")]
    public async Task<IActionResult> Archive(int id)
    {
        var result = await service.ArchiveTaskAsync(id, User.GetUserId()!);
        return result.IsSuccess ? Ok(new { message = "Task archived successfully" }) : result.ToProblem();
    }

    // PUT api/tasks/{id}/restore
    [HttpPut("{id:int}/restore")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Restore(int id)
    {
        var result = await service.RestoreTaskAsync(id, User.GetUserId()!);
        return result.IsSuccess ? Ok(new { message = "Task restored successfully" }) : result.ToProblem();
    }

    // POST api/tasks/{id}/assignees
    [HttpPost("{id:int}/assignees")]
    public async Task<IActionResult> AssignUser(int id, [FromBody] AssignUserRequest request)
    {
        var result = await service.AssignUserAsync(id, request.UserId, User.GetUserId()!);
        return result.IsSuccess ? Ok(new { message = "User assigned successfully" }) : result.ToProblem();
    }

    // DELETE api/tasks/{id}/assignees/{userId}
    [HttpDelete("{id:int}/assignees/{userId}")]
    public async Task<IActionResult> UnassignUser(int id, string userId)
    {
        var result = await service.UnassignUserAsync(id, userId);
        return result.IsSuccess ? NoContent() : result.ToProblem();
    }

    // POST api/tasks/{id}/files
    [HttpPost("{id:int}/files")]
    public async Task<IActionResult> UploadFile(int id, IFormFile file, [FromQuery] int? occurrenceId = null)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { message = "No file provided" });

        var result = await service.UploadFileAsync(id, file, User.GetUserId()!, occurrenceId);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    // DELETE api/tasks/{id}/files/{fileId}
    [HttpDelete("{id:int}/files/{fileId:int}")]
    public async Task<IActionResult> DeleteFile(int id, int fileId)
    {
        var UserId = User.GetUserId();
        var result = await service.DeleteFileAsync(id, fileId,UserId!);
        return result.IsSuccess ? NoContent() : result.ToProblem();
    }


    // GET api/tasks/{id}/occurrences
    [HttpGet("{id:int}/occurrences")]
    public async Task<IActionResult> GetOccurrences(int id)
    {
        var result = await service.GetOccurrencesAsync(id);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    // PATCH api/tasks/{id}/occurrences/{occurrenceId}
    [HttpPatch("{id:int}/occurrences/{occurrenceId:int}")]
    public async Task<IActionResult> UpdateOccurrence(
        int id, int occurrenceId, [FromBody] UpdateOccurrenceRequest request)
    {
        var result = await service.UpdateOccurrenceAsync(id, occurrenceId, request, User.GetUserId()!);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }
}