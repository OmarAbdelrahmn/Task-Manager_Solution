using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class addseedintgs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "IsDefault", "IsDeleted", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "77B96C5D-F502-47TF-EE95-ABVN14A3CA22", "A7B75EE9-DB35-480D-9F9F-18D2E499B004", true, false, "User", "USER" },
                    { "77B96CED-F902-47EF-AE95-ABBE14A8CA22", "B0AD2D39-253B-42E4-88F2-F6FE83A614A8", false, false, "Admin", "ADMIN" }
                });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "Address", "ConcurrencyStamp", "Email", "EmailConfirmed", "FullName", "IsDisabled", "LastLogin", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[,]
                {
                    { "59724D2D-E2B5-4C67-AB6F-D93478347B03", 0, "", "B4555410-F5B0-45B1-B963-1B2351A0723C", null, true, "", false, null, false, null, null, "ADMIN", "AQAAAAIAAYagAAAAEAIUBEJvEUbyeQ5VMd8P/E8OrHS1EsXJ1NpHW320SL50j67p9Y8s1jVeOpz/OLR/IQ==", null, false, "9FABB58491024B7BB140E4D6658B5BDA", false, "Admin" },
                    { "59726D2D-E2B5-4C67-AB6F-D93478317B03", 0, "", "B4555410-F5B0-45B1-B963-1B2351A0723C", null, true, "", false, null, false, null, null, "USER", "AQAAAAIAAYagAAAAEPBNuKnUlvEBErBqcYL2s6WRt7sYQ3v3D5Ovu1FTP/NmH05291AucW/PC+X1Ajrsig==", null, false, "9FABB58491024B7BB140E4D6658B5BDA", false, "User" }
                });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[,]
                {
                    { "77B96CED-F902-47EF-AE95-ABBE14A8CA22", "59724D2D-E2B5-4C67-AB6F-D93478347B03" },
                    { "77B96C5D-F502-47TF-EE95-ABVN14A3CA22", "59726D2D-E2B5-4C67-AB6F-D93478317B03" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "77B96CED-F902-47EF-AE95-ABBE14A8CA22", "59724D2D-E2B5-4C67-AB6F-D93478347B03" });

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "77B96C5D-F502-47TF-EE95-ABVN14A3CA22", "59726D2D-E2B5-4C67-AB6F-D93478317B03" });

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "77B96C5D-F502-47TF-EE95-ABVN14A3CA22");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "77B96CED-F902-47EF-AE95-ABBE14A8CA22");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59724D2D-E2B5-4C67-AB6F-D93478347B03");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "59726D2D-E2B5-4C67-AB6F-D93478317B03");
        }
    }
}
