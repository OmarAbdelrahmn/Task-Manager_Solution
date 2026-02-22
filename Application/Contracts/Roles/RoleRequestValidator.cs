using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Contracts.Roles;

public class RoleRequestValidator : AbstractValidator<RoleRequest>
{
    public RoleRequestValidator()
    {
        RuleFor(i => i.OldName)
            .NotEmpty()
            .Length(3, 256);

        RuleFor(i => i.NewName)
            .NotEmpty()
            .Length(3, 256);


    }
}
