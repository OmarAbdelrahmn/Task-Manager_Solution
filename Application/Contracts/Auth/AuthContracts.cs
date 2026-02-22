using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Contracts.Auth;

internal class AuthContracts
{
}
public record AuthResponse(
    string Id,
    string UserName,
    string? FullName,
    string Token,
    int ExpiresIn,
    string RefreshToken,
    DateTime RefreshTokenExpiration
);

public record RegisterRequest(
    string UserName,
    string Password,
    string FullName
);

public record ConfirmEmailRequest(
    string UserId,
    string Code
);

public record ResendConfirmationEmailRequest(
    string Email
);

public record ResetPasswordRequest(
    string Email,
    string Code,
    string NewPassword
);