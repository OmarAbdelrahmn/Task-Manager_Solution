using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Contracts.User;

internal class UserContracts
{
}

public record UserProfileResponse
(
    string UserName,
    string FullName,
    string Address
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
