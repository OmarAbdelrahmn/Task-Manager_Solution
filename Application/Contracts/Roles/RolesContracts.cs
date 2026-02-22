using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Contracts.Roles;

internal class RolesContracts
{
}
public record RoleDetailsResponse
(
    string Id,
    string Name,
    bool IsDeleted
    );

public record RoleRequest
(
    string OldName,
    string NewName
    );

public record RolesResponse
(
    string Id,
    string Name,
    bool IsDeleted
    );
