using Application.Contracts.Auth;
using Application.Extensions;
using Application.Services.Auth;
using Application.Services.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Task_Manager.Controllers;

[Route("api/auth")]
[ApiController]
public class AuthController(
    IAuthService service,
    IUserService userService) : ControllerBase
{
    // POST api/auth/register
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await service.RegisterAsync(request);
        return result.IsSuccess
            ? Ok(new { message = "Registered successfully, please sign in" })
            : result.ToProblem();
    }

    // POST api/auth/signin
    [HttpPost("signin")]
    public async Task<IActionResult> SignIn([FromBody] LoginRequsst request)
    {
        var result = await service.GetTokenAsync(request.UserName, request.Password);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    // POST api/auth/refresh
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(
        [FromBody] RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        var result = await service.GetRefreshTokenAsync(
            request.Token, request.RefreshToken, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    // POST api/auth/logout
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout(
        [FromBody] RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        // revoke token
        var result = await service.RevokeRefreshTokenAsync(
            request.Token, request.RefreshToken, cancellationToken);

        if (!result.IsSuccess)
            return result.ToProblem();

        // ✅ mark user offline
        await userService.SetOnlineStatusAsync(User.GetUserId()!, false);

        return Ok(new { message = "Logged out successfully" });
    }
}