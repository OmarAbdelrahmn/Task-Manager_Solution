using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Contracts.Admin;

internal class AdminContracts
{
}

public record UserResponse
(
    string Id,
    string FullName,
    string Address,
    string UserName,
    bool IsDisable
    );

public record UserResponses
(
    string Id,
    string FullName,
    string Address,
    string UserName,
    bool IsDisable,
    IEnumerable<string> Roles,
    DateTime? LastLogin
    );
