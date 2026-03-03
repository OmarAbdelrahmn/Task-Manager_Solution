// Application/Services/User/UserServices.cs
using Application.Abstraction;
using Application.Abstraction.Errors;
using Application.Contracts.User;
using Application.Services.Roles;
using Application.Services.UserFile;
using Domain.Entities.Identity;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.User;

public class UserServices(
    UserManager<ApplicationUser> manager,
    IHttpContextAccessor httpContextAccessor) : IUserService
{
    public async Task<Result> ChangePassword(string id, ChangePasswordRequest request)
    {
        var user = await manager.FindByIdAsync(id);

        if (user is null)
            return Result.Failure(UserErrors.UserNotFound);

        var result = await manager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassord);

        if (result.Succeeded)
            return Result.Success();

        var error = result.Errors.First();
        return Result.Failure(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
    }

    public async Task<Result> ChangeRoleForUser(string userName, string newRole)
    {
        var user = await manager.FindByNameAsync(userName);

        if (user is null)
            return Result.Failure(UserErrors.UserNotFound);

        var roles = await manager.GetRolesAsync(user);

        if (roles == null || roles.Count == 0)
            return Result.Failure(RolesErrors.somethingwrong);

        if (roles.Contains(newRole))
            return Result.Failure(RolesErrors.haveit);

        var removeResult = await manager.RemoveFromRolesAsync(user, roles);
        if (!removeResult.Succeeded)
            return Result.Failure(RolesErrors.somethingwrong);

        var addResult = await manager.AddToRoleAsync(user, newRole);
        if (!addResult.Succeeded)
            return Result.Failure(RolesErrors.somethingwrong);

        return Result.Success();
    }

    public async Task<Result<UserProfileResponse>> GetUserProfile(string id)
    {
        var user = await manager.Users
            .Where(u => u.Id == id)
            .ProjectToType<UserProfileResponse>()
            .SingleOrDefaultAsync();

        if (user is null)
            return Result.Failure<UserProfileResponse>(UserErrors.UserNotFound);

        return Result.Success(user);
    }

    public async Task<Result> UpdateUserProfile(string id, UpdateUserProfileRequest request)
    {
        var user = await manager.FindByIdAsync(id);

        if (user is null)
            return Result.Failure(UserErrors.UserNotFound);

        await manager.Users
            .Where(u => u.Id == id)
            .ExecuteUpdateAsync(set => set
                .SetProperty(x => x.FullName, request.FullName)
                .SetProperty(x => x.Address, request.Address));

        return Result.Success();
    }

    public async Task<Result<string>> UpdateAvatarAsync(string id, IFormFile file, IUserFileService fileService)
    {
        var user = await manager.FindByIdAsync(id);

        if (user is null)
            return Result.Failure<string>(UserErrors.UserNotFound);

        // delete old avatar if exists
        if (!string.IsNullOrEmpty(user.AvatarUrl))
            fileService.DeleteFile(user.AvatarUrl);

        var avatarUrl = await fileService.SaveFileAsync(file, "avatars",id);

        await manager.Users
            .Where(u => u.Id == id)
            .ExecuteUpdateAsync(set => set
                .SetProperty(x => x.AvatarUrl, avatarUrl));

        return Result.Success(avatarUrl);
    }

    public async Task<Result> SetOnlineStatusAsync(string id, bool isOnline)
    {
        var user = await manager.FindByIdAsync(id);

        if (user is null)
            return Result.Failure(UserErrors.UserNotFound);

        await manager.Users
            .Where(u => u.Id == id)
            .ExecuteUpdateAsync(set => set
                .SetProperty(x => x.IsOnline, isOnline)
                .SetProperty(x => x.LastLogin, isOnline ? DateTime.Now : user.LastLogin));

        return Result.Success();
    }

    // ── NEW — append inside the UserServices class ───────────────────────────────

    public async Task<Result<IEnumerable<UserAssigneeResponse>>> GetAssignableUsersAsync(
        string? search = null,
        IEnumerable<string>? excludeIds = null)
    {
        var query = manager.Users
            .Where(u => !u.IsDisabled);          // only active accounts


        var users = await query
            .OrderBy(u => u.FullName)
            .Select(u => new UserAssigneeResponse(
                u.Id,
                u.UserName ?? "",
                u.FullName ?? "",
                u.AvatarUrl ?? "" ,
                u.IsOnline))
            .ToListAsync();

        return Result.Success<IEnumerable<UserAssigneeResponse>>(users);
    }

    public async Task<Result<bool>> IsUserNameAvailableAsync(string userName)
    {
        if (string.IsNullOrWhiteSpace(userName))
            return Result.Failure<bool>(UserErrors.InvalidCredentials);

        var exists = await manager.FindByNameAsync(userName.Trim());
        return Result.Success(exists is null);   // true = available, false = taken
    }
}