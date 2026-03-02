using Application.Contracts.User;
using Application.Extensions;
using Application.Services.Auth;
using Application.Services.Roles;
using Application.Services.User;
using Application.Services.UserFile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Task_Manager;

namespace Task_Manager.Controllers;

[Route("api/me")]
[ApiController]
[Authorize]
public class AccountController(
    IUserService service,
    IUserFileService fileService) : ControllerBase
{

    [HttpGet("assignable")]
    [Authorize]                          // any authenticated user can call this
    public async Task<IActionResult> GetAssignableUsers(
    [FromQuery] string? search,
    [FromQuery] IEnumerable<string>? excludeIds)
    {
        var result = await service.GetAssignableUsersAsync(search, excludeIds);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("check-username")]
    [AllowAnonymous]
    public async Task<IActionResult> CheckUserName([FromQuery] string userName)
    {
        var result = await service.IsUserNameAvailableAsync(userName);
        return result.IsSuccess
            ? Ok(new { isAvailable = result.Value })
            : result.ToProblem();
    }

    [HttpGet("")]
    public async Task<IActionResult> GetProfile()
    {
        var result = await service.GetUserProfile(User.GetUserId()!);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPut("info")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserProfileRequest request)
    {
        var result = await service.UpdateUserProfile(User.GetUserId()!, request);
        return result.IsSuccess ? Ok(new { message = "Profile updated successfully" }) : result.ToProblem();
    }

    [HttpPut("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var result = await service.ChangePassword(User.GetUserId()!, request);
        return result.IsSuccess ? Ok(new { message = "Password changed successfully" }) : result.ToProblem();
    }

    [HttpPut("avatar")]
    public async Task<IActionResult> UpdateAvatar(IFormFile file)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { message = "No file provided" });

        if (!fileService.IsValidAvatar(file))
            return BadRequest(new { message = "Invalid file. Only JPG, PNG, WEBP allowed. Max size 2MB" });

        var result = await service.UpdateAvatarAsync(User.GetUserId()!, file, fileService);
        return result.IsSuccess ? Ok(new { message = "Avatar updated successfully" }) : result.ToProblem();
    }
}