using Domain.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Authentication;

public interface IJwtProvider
{
    (string token, int expiresIn) GenerateToken(ApplicationUser user, IEnumerable<string> roles);
    string? ValidateToken(string token);
}