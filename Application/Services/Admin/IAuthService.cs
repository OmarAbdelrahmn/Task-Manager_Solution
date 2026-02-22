using Application.Abstraction;
using Application.Contracts.Admin;
using Application.Contracts.Auth;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Services.Admin;

public interface IAdminService
{
    Task<IEnumerable<UserResponses>> GetAllUsers();
    Task<Result<UserResponses>> GetUserAsync(string Id);
    Task<Result<UserResponses>> GetUser2Async(string UserName);
    Task<Result> ToggleStatusAsync(string UserName);
    Task<Result> DeletaUserAsync(string UserName);

    Task<Result> ResetPasswordAsync(string userName);
}
