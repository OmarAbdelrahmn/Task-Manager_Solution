
using Application.Contracts.Conversations;
using Application.Extensions;
using Application.Services.Conversations;
using Application.Services.UserFile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Task_Manager;

[Route("api/conversations")]
[ApiController]
[Authorize]
public class ConversationsController(IConversationService service) : ControllerBase
{
    // GET api/conversations
    [HttpGet]
    public async Task<IActionResult> GetMine()
    {
        var result = await service.GetMyConversationsAsync(User.GetUserId()!);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    // GET api/conversations/{id}
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await service.GetConversationAsync(id, User.GetUserId()!);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    // POST api/conversations/direct
    [HttpPost("direct")]
    public async Task<IActionResult> CreateDirect([FromBody] CreateDirectConversationRequest req)
    {
        var result = await service.CreateDirectAsync(req, User.GetUserId()!);
        return result.IsSuccess ? CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value) : result.ToProblem();
    }

    // POST api/conversations/group
    [HttpPost("group")]
    public async Task<IActionResult> CreateGroup([FromBody] CreateGroupConversationRequest req)
    {
        var result = await service.CreateGroupAsync(req, User.GetUserId()!);
        return result.IsSuccess ? CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value) : result.ToProblem();
    }

    // POST api/conversations/{id}/participants
    [HttpPost("{id:int}/participants")]
    public async Task<IActionResult> AddParticipant(int id, [FromBody] string userId)
    {
        var result = await service.AddParticipantAsync(id, userId, User.GetUserId()!);
        return result.IsSuccess ? Ok(new { message = "Participant added." }) : result.ToProblem();
    }

    // DELETE api/conversations/{id}/participants/{userId}
    [HttpDelete("{id:int}/participants/{userId}")]
    public async Task<IActionResult> RemoveParticipant(int id, string userId)
    {
        var result = await service.RemoveParticipantAsync(id, userId, User.GetUserId()!);
        return result.IsSuccess ? NoContent() : result.ToProblem();
    }

    // POST api/conversations/{id}/read
    [HttpPost("{id:int}/read")]
    public async Task<IActionResult> MarkRead(int id)
    {
        var result = await service.MarkAsReadAsync(id, User.GetUserId()!);
        return result.IsSuccess ? Ok() : result.ToProblem();
    }

    // GET api/conversations/{id}/messages?page=1&pageSize=50
    [HttpGet("{id:int}/messages")]
    public async Task<IActionResult> GetMessages(int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var result = await service.GetMessagesAsync(id, User.GetUserId()!, page, pageSize);
        return Ok(result);
    }

    // POST api/conversations/messages
    [HttpPost("messages")]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest req)
    {
        var result = await service.SendMessageAsync(req, User.GetUserId()!);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    // PUT api/conversations/messages/{messageId}
    [HttpPut("messages/{messageId:int}")]
    public async Task<IActionResult> EditMessage(int messageId, [FromBody] EditMessageRequest req)
    {
        var result = await service.EditMessageAsync(messageId, req, User.GetUserId()!);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    // DELETE api/conversations/messages/{messageId}
    [HttpDelete("messages/{messageId:int}")]
    public async Task<IActionResult> DeleteMessage(int messageId)
    {
        var result = await service.DeleteMessageAsync(messageId, User.GetUserId()!);
        return result.IsSuccess ? NoContent() : result.ToProblem();
    }

    // POST api/conversations/{id}/files
    [HttpPost("{id:int}/files")]
    public async Task<IActionResult> UploadFile(int id, IFormFile file)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { message = "No file provided." });

        var result = await service.UploadMessageFileAsync(id, file, User.GetUserId()!);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    // Task_Manager/Controllers/ConversationsController.cs  (new endpoints — add to existing class)

    // ── Update group name ──────────────────────────────────────────────────
    // PUT api/conversations/{id}
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateGroup(int id, [FromBody] UpdateGroupRequest req)
    {
        var result = await service.UpdateGroupAsync(id, req, User.GetUserId()!);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    // ── Leave conversation ─────────────────────────────────────────────────
    // POST api/conversations/{id}/leave
    [HttpPost("{id:int}/leave")]
    public async Task<IActionResult> Leave(int id)
    {
        var result = await service.LeaveConversationAsync(id, User.GetUserId()!);
        return result.IsSuccess ? NoContent() : result.ToProblem();
    }

    // ── Delete conversation ────────────────────────────────────────────────
    // DELETE api/conversations/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await service.DeleteConversationAsync(id, User.GetUserId()!);
        return result.IsSuccess ? NoContent() : result.ToProblem();
    }

    // ── Total unread count ─────────────────────────────────────────────────
    // GET api/conversations/unread
    [HttpGet("unread")]
    public async Task<IActionResult> GetUnreadCount()
    {
        var result = await service.GetUnreadCountAsync(User.GetUserId()!);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    // ── Search messages ────────────────────────────────────────────────────
    // GET api/conversations/{id}/messages/search?query=hello&page=1&pageSize=30
    [HttpGet("{id:int}/messages/search")]
    public async Task<IActionResult> SearchMessages(
        int id,
        [FromQuery] string query,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 30)
    {
        if (string.IsNullOrWhiteSpace(query))
            return BadRequest(new { message = "Query cannot be empty." });

        var result = await service.SearchMessagesAsync(id, User.GetUserId()!,
            new MessageSearchRequest(query, page, pageSize));
        return Ok(result);
    }

    // ── Get reactions for a message ────────────────────────────────────────
    // GET api/conversations/messages/{messageId}/reactions
    [HttpGet("messages/{messageId:int}/reactions")]
    public async Task<IActionResult> GetReactions(int messageId)
    {
        var result = await service.GetReactionsAsync(messageId, User.GetUserId()!);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    // ── Add reaction ───────────────────────────────────────────────────────
    // POST api/conversations/messages/{messageId}/reactions
    [HttpPost("messages/{messageId:int}/reactions")]
    public async Task<IActionResult> React(int messageId, [FromBody] ReactRequest req)
    {
        var result = await service.ReactAsync(messageId, req, User.GetUserId()!);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    // ── Remove reaction ────────────────────────────────────────────────────
    // DELETE api/conversations/messages/{messageId}/reactions/{emoji}
    [HttpDelete("messages/{messageId:int}/reactions/{emoji}")]
    public async Task<IActionResult> RemoveReaction(int messageId, string emoji)
    {
        var result = await service.RemoveReactionAsync(messageId, emoji, User.GetUserId()!);
        return result.IsSuccess ? NoContent() : result.ToProblem();
    }

    // PUT api/conversations/{id}/avatar
    [HttpPut("{id:int}/avatar")]
    public async Task<IActionResult> UpdateAvatar(
        int id,
        IFormFile file,
        [FromServices] IUserFileService fileService)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { message = "No file provided." });

        if (!fileService.IsValidAvatar(file))
            return BadRequest(new { message = "Invalid file. Only JPG, PNG, WEBP allowed. Max 5MB." });

        var result = await service.UpdateConversationAvatarAsync(
            id, file, User.GetUserId()!, fileService);

        return result.IsSuccess
            ? Ok(new { avatarUrl = result.Value })
            : result.ToProblem();
    }
}
