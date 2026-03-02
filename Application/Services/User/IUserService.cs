// Application/Services/User/IUserService.cs
using Application.Abstraction;
using Application.Contracts.User;
using Application.Services.Roles;
using Application.Services.UserFile;
using Microsoft.AspNetCore.Http;

namespace Application.Services.User;

public interface IUserService
{
    Task<Result<UserProfileResponse>> GetUserProfile(string id);
    Task<Result> UpdateUserProfile(string id, UpdateUserProfileRequest request);
    Task<Result> ChangePassword(string id, ChangePasswordRequest request);
    Task<Result> ChangeRoleForUser(string userName, string newRole);
    Task<Result<string>> UpdateAvatarAsync(string id, IFormFile file, IUserFileService fileService);
    Task<Result> SetOnlineStatusAsync(string id, bool isOnline);

    Task<Result<IEnumerable<UserAssigneeResponse>>> GetAssignableUsersAsync(
    string? search = null,
    IEnumerable<string>? excludeIds = null);

    Task<Result<bool>> IsUserNameAvailableAsync(string userName);
}