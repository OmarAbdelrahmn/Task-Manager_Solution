using Application.Contracts.Auth;
using Application.Services.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Task_Manager;

namespace Task_Manager.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController(IAuthService service) : ControllerBase
{
    private readonly IAuthService service = service;

    [HttpPost("register")] 
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var response = await service.RegisterAsync(request);

        return response.IsSuccess ?
            Ok(new Resu("Done please try to Login")) :
            response.ToProblem();
    }
    
    [HttpPost("signin")] 
    public async Task<IActionResult> Signin([FromBody] LoginRequsst request)
    {
        var response = await service.GetTokenAsync(request.UserName , request.Password);

        return response.IsSuccess ?
            Ok(response.Value) :
            response.ToProblem();
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var authResult = await service.GetRefreshTokenAsync(request.Token, request.RefreshToken, cancellationToken);

        return authResult.IsSuccess ? Ok(authResult.Value) : authResult.ToProblem();
    }

    [HttpPost("revoke-refresh-token")]
    public async Task<IActionResult> RevokeRefreshToken([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var result = await service.RevokeRefreshTokenAsync(request.Token, request.RefreshToken, cancellationToken);

        return result.IsSuccess ? Ok() : result.ToProblem();
    }

    public class Resu(string massage)
    {
        public string Massage { get; set; } = massage;
    }
}