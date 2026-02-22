using Domain.Entities;
using Domain.Entities.Identity;
using Domain.Entities.Seeding;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.EntitiesConfigrations;

public class UserConfigration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.HasData(new ApplicationUser
        {
            Id = DefaultUsers.AdminId,
            UserName = DefaultUsers.AdminName,
            NormalizedUserName = DefaultUsers.AdminName.ToUpper(),
            EmailConfirmed = true,
            PasswordHash = new PasswordHasher<ApplicationUser>().HashPassword(null!, "P@ssword1234"),
            SecurityStamp = DefaultUsers.AdminSecurityStamp,
            ConcurrencyStamp = DefaultUsers.AdminConcurrencyStamp
        });
        
        builder.HasData(new ApplicationUser
        {
            Id = DefaultUsers.UserId,
            UserName = DefaultUsers.UserName,
            NormalizedUserName = DefaultUsers.UserName.ToUpper(),
            EmailConfirmed = true,
            PasswordHash = new PasswordHasher<ApplicationUser>().HashPassword(null!, "P@ssword1234"),
            SecurityStamp = DefaultUsers.UserSecurityStamp,
            ConcurrencyStamp = DefaultUsers.UserConcurrencyStamp
        });

    }
}

