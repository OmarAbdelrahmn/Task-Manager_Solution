using Domain.Entities.Seeding;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.EntitiesConfigrations;

public class UserRolesConfigration : IEntityTypeConfiguration<IdentityUserRole<string>>
{
    public void Configure(EntityTypeBuilder<IdentityUserRole<string>> builder)
    {

        builder.HasData(
            new IdentityUserRole<string>
            {
                UserId = DefaultUsers.AdminId,
                RoleId = DefaultRoles.AdminRoleId
            },
            new IdentityUserRole<string>
            {
                UserId = DefaultUsers.UserId,
                RoleId = DefaultRoles.UserRoleId
            }


        );

    }
}

