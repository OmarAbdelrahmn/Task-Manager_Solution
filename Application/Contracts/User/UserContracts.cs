using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Contracts.User;

internal class UserContracts
{
}

public record UserProfileResponse
(
    string Id,
    string UserName,
    string FullName,
    string Address,
    string? AvatarUrl
    );


public record UpdateUserProfileRequest
(
    string FullName,
    string Address
    );

public record ChangePasswordRequest
(
    string CurrentPassword,
    string NewPassord
    );

/// <summary>
/// Lightweight user projection used in "assign task" dropdowns / pickers.
/// </summary>
public record UserAssigneeResponse(
    string Id,
    string UserName,
    string FullName,
    string AvatarUrl,
    bool IsOnline
);
