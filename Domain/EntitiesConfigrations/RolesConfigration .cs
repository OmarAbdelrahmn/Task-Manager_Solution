using Domain.Entities;
using Domain.Entities.Identity;
using Domain.Entities.Seeding;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.EntitiesConfigrations;

public class RolesConfigration : IEntityTypeConfiguration<ApplicationRole>
{
    public void Configure(EntityTypeBuilder<ApplicationRole> builder)
    {

        builder.HasData(
            [
                new ApplicationRole
                {
                    Id = DefaultRoles.AdminRoleId,
                    Name = DefaultRoles.Admin,
                    ConcurrencyStamp = DefaultRoles.AdminRoleConcurrencyStamp,
                    NormalizedName = DefaultRoles.Admin.ToUpper(),
                    IsDefault = false,
                    IsDeleted = false
                },
                new ApplicationRole
                {
                    Id = DefaultRoles.UserRoleId,
                    Name = DefaultRoles.User,
                    ConcurrencyStamp = DefaultRoles.UserRoleConcurrencyStamp,
                    NormalizedName = DefaultRoles.User.ToUpper(),
                    IsDefault = true,
                    IsDeleted = false
                }
            ]
        );

    }
}

