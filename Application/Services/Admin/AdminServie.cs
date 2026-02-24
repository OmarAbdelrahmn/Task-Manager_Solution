// Application/Services/Admin/AdminService.cs
using Application.Abstraction;
using Application.Abstraction.Errors;
using Application.Contracts.Admin;
using Domain;
using Domain.Entities.Identity;
using Domain.Entities.Main;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.Admin;

public class AdminService(
    UserManager<ApplicationUser> manager,
    ApplicationDbcontext dbcontext) : IAdminService
{
    public async Task<Result> ResetPasswordAsync(string userName)
    {
        var user = await manager.FindByNameAsync(userName);

        if (user is null)
            return Result.Failure(UserErrors.UserNotFound);

        var removeResult = await manager.RemovePasswordAsync(user);
        if (!removeResult.Succeeded)
        {
            var err = removeResult.Errors.First();
            return Result.Failure(new Error(err.Code, err.Description, StatusCodes.Status400BadRequest));
        }

        var addResult = await manager.AddPasswordAsync(user, "P@ssword1234");
        if (!addResult.Succeeded)
        {
            var err = addResult.Errors.First();
            return Result.Failure(new Error(err.Code, err.Description, StatusCodes.Status400BadRequest));
        }

        return Result.Success();
    }

    public async Task<IEnumerable<UserResponses>> GetAllUsers()
    {
        var users = await manager.Users
            .Select(u => new
            {
                u.Id,
                u.UserName,
                u.FullName,
                u.Address,
                u.IsDisabled,
                u.IsOnline,
                u.LastLogin,
                u.AvatarUrl,
                // ✅ task stats per user
                AssignedTasksCount = dbcontext.TaskAssignees
                    .Count(ta => ta.UserId == u.Id),
                CompletedTasksCount = dbcontext.TaskAssignees
                    .Count(ta => ta.UserId == u.Id
                        && ta.Task.Status == Domain.Entities.TaskStatus.Done)
            })
            .ToListAsync();

        var result = new List<UserResponses>();

        foreach (var u in users)
        {
            var roles = await manager.GetRolesAsync(
                await manager.FindByIdAsync(u.Id) ?? new ApplicationUser());

            result.Add(new UserResponses(
                u.Id,
                u.FullName ?? "",
                u.Address ?? "",
                u.UserName ?? "",
                u.IsDisabled,
                u.IsOnline,
                u.AvatarUrl,
                roles,
                u.LastLogin,
                u.AssignedTasksCount,
                u.CompletedTasksCount
            ));
        }

        return result;
    }

    public async Task<Result<UserResponses>> GetUserAsync(string id)
    {
        if (await manager.FindByIdAsync(id) is not { } user)
            return Result.Failure<UserResponses>(UserErrors.UserNotFound);

        var roles = await manager.GetRolesAsync(user);

        var assignedCount = await dbcontext.TaskAssignees
            .CountAsync(ta => ta.UserId == id);

        var completedCount = await dbcontext.TaskAssignees
            .CountAsync(ta => ta.UserId == id
                && ta.Task.Status == Domain.Entities.TaskStatus.Done);

        var response = new UserResponses(
            user.Id, user.FullName ?? "", user.Address ?? "",
            user.UserName ?? "", user.IsDisabled, user.IsOnline,
            user.AvatarUrl, roles, user.LastLogin,
            assignedCount, completedCount);

        return Result.Success(response);
    }

    public async Task<Result<UserResponses>> GetUser2Async(string userName)
    {
        if (await manager.FindByNameAsync(userName) is not { } user)
            return Result.Failure<UserResponses>(UserErrors.UserNotFound);

        var roles = await manager.GetRolesAsync(user);

        var assignedCount = await dbcontext.TaskAssignees
            .CountAsync(ta => ta.UserId == user.Id);

        var completedCount = await dbcontext.TaskAssignees
            .CountAsync(ta => ta.UserId == user.Id
                && ta.Task.Status == Domain.Entities.TaskStatus.Done);

        var response = new UserResponses(
            user.Id, user.FullName ?? "", user.Address ?? "",
            user.UserName ?? "", user.IsDisabled, user.IsOnline,
            user.AvatarUrl, roles, user.LastLogin,
            assignedCount, completedCount);

        return Result.Success(response);
    }

    public async Task<Result> ToggleStatusAsync(string userName)
    {
        if (await manager.FindByNameAsync(userName) is not { } user)
            return Result.Failure(UserErrors.UserNotFound);

        user.IsDisabled = !user.IsDisabled;

        // ✅ force offline when disabling
        if (user.IsDisabled)
            user.IsOnline = false;

        var result = await manager.UpdateAsync(user);

        if (result.Succeeded)
            return Result.Success();

        var error = result.Errors.First();
        return Result.Failure(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
    }

    public async Task<Result> DeletaUserAsync(string userName)
    {
        if (await manager.FindByNameAsync(userName) is not { } user)
            return Result.Failure(UserErrors.UserNotFound);

        // ✅ check if user has active tasks before deleting
        var hasActiveTasks = await dbcontext.TaskAssignees
            .AnyAsync(ta => ta.UserId == user.Id
                && ta.Task.Status != Domain.Entities.TaskStatus.Done
                && ta.Task.Status != Domain.Entities.TaskStatus.Archived);

        if (hasActiveTasks)
            return Result.Failure(UserErrors.HasActiveTasks);

        var result = await manager.DeleteAsync(user);

        if (result.Succeeded)
            return Result.Success();

        var error = result.Errors.First();
        return Result.Failure(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
    }
}