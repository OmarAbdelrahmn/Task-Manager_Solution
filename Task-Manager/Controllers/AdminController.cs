
using Application.Services.Admin;
using Application.Services.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR.Protocol;



namespace Task_Manager.Controllers;
[Route("api/[controller]")]
[ApiController]
public class AdminController(IAdminService service,IUserService service1) : ControllerBase
{
    private readonly IAdminService service = service;
    private readonly IUserService service1 = service1;



    [HttpGet("adminreset")]
    [Authorize(Roles = "Master")]
    public async Task<IActionResult> AdminReset(string UserName)
    {
        var result = await service.ResetPasswordAsync(UserName);
        return result.IsSuccess ?
            Ok() :
            result.ToProblem();
    }

    [HttpGet("users")]
    [Authorize(Roles = "Master")]
    public async Task<IActionResult> GetUsers()
    {
        var users = await service.GetAllUsers();

        return users is not null ?
            Ok(users) :
            BadRequest();
    }

    [HttpPost("users/role")]
    [Authorize(Roles = "Master")]
 

    public async Task<IActionResult> ChangeRoles([FromBody] Rer request)
    {
        var result = await service1.ChangeRoleForUser(request.UserName, request.NewRole);

        return result.IsSuccess ? Ok(new Re("Role updated successfully")) : result.ToProblem();
    }

    [HttpGet("users/id/{Id}")]
    [Authorize(Roles = "Master")]
 

    public async Task<IActionResult> GetUser(string Id)
    {
        var user = await service.GetUserAsync(Id);

        return user.IsSuccess ?
            Ok(user.Value) :
            user.ToProblem();
    }
    
    [HttpGet("users/name/{UserName}")]
    [Authorize(Roles = "Master")]
 

    public async Task<IActionResult> GetUser2(string UserName)
    {
        var user = await service.GetUser2Async(UserName);

        return user.IsSuccess ?
            Ok(user.Value) :
            user.ToProblem();
    }

    [HttpPut("users/{UserName}/toggle-status")]
    [Authorize(Roles = "Master")]
 

    public async Task<IActionResult> ToggleStatusAsync(string UserName)
    {
        var user = await service.ToggleStatusAsync(UserName);
        return user.IsSuccess ?
            NoContent() :
            user.ToProblem();
    }


    [HttpDelete("users/{UserName}")]
    public async Task<IActionResult> DeleteAsync(string UserName)
    {
        var user = await service.DeletaUserAsync(UserName);

        return user.IsSuccess ?
            Ok(new Re("done")) :
            user.ToProblem();
    }


}


public record Rer(string UserName,string NewRole);

public record Re(string massege);
