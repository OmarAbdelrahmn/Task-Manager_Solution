using Application.Services.Admin;
using Application.Services.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Task_Manager.Controllers;

[Route("api/admin")]
[ApiController]
[Authorize(Roles = "Admin")]       
public class AdminController(
    IAdminService service,
    IUserService userService) : ControllerBase
{
    // GET api/admin/users
    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await service.GetAllUsers();
        return Ok(users);
    }

    // GET api/admin/users/{id}
    [HttpGet("users/{id}")]
    public async Task<IActionResult> GetUserById(string id)
    {
        var result = await service.GetUserAsync(id);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    // GET api/admin/users/by-name/{userName}
    [HttpGet("users/by-name/{userName}")]
    public async Task<IActionResult> GetUserByName(string userName)
    {
        var result = await service.GetUser2Async(userName);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    // PUT api/admin/users/{userName}/toggle-status
    [HttpPut("users/{userName}/toggle-status")]
    public async Task<IActionResult> ToggleStatus(string userName)
    {
        var result = await service.ToggleStatusAsync(userName);
        return result.IsSuccess ? NoContent() : result.ToProblem();
    }

    // DELETE api/admin/users/{userName}
    [HttpDelete("users/{userName}")]
    public async Task<IActionResult> DeleteUser(string userName)
    {
        var result = await service.DeletaUserAsync(userName);
        return result.IsSuccess ? Ok(new { message = "User deleted successfully" }) : result.ToProblem();
    }

    // POST api/admin/users/{userName}/reset-password
    [HttpPost("users/{userName}/reset-password")]
    public async Task<IActionResult> ResetPassword(string userName)
    {
        var result = await service.ResetPasswordAsync(userName);
        return result.IsSuccess ? Ok(new { message = "Password reset to P@ssword1234" }) : result.ToProblem();
    }

    // PUT api/admin/users/role
    [HttpPut("users/role")]
    public async Task<IActionResult> ChangeRole([FromBody] ChangeRoleRequest request)
    {
        var result = await userService.ChangeRoleForUser(request.UserName, request.NewRole);
        return result.IsSuccess ? Ok(new { message = "Role updated successfully" }) : result.ToProblem();
    }
}

public record ChangeRoleRequest(string UserName, string NewRole);