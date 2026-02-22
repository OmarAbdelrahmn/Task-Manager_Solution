using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Entities.Seeding;

public static class DefaultRoles
{
    public const string AdminRoleId = "77B96CED-F902-47EF-AE95-ABBE14A8CA22";
    public const string AdminRoleConcurrencyStamp = "B0AD2D39-253B-42E4-88F2-F6FE83A614A8";
    public const string Admin = nameof(Admin);

    public const string UserRoleConcurrencyStamp = "A7B75EE9-DB35-480D-9F9F-18D2E499B004";
    public const string UserRoleId = "77B96C5D-F502-47TF-EE95-ABVN14A3CA22";
    public const string User = nameof(User);
}
