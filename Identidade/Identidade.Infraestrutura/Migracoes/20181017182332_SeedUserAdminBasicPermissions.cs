using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Identidade.Infraestrutura.Migrations
{
    public partial class SeedUserAdminBasicPermissions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "CreatedAt", "Email", "EmailConfirmed", "LastUpdatedAt", "LockoutEnabled", "LockoutEnd", "Name", "NormalizedEmail", "NormalizedUserName", "PasswordDoesNotExpire", "PasswordHash", "PasswordHistory", "PasswordMaturity", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserId", "UserName" },
                values: new object[] { "00000000-0000-0000-0000-000000000000", 0, "5ff78f47-9190-4c1f-9ba5-bb3dd87c7a6f", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, false, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, null, "Administrator", null, null, null, "$2b$10$YJ7ApONRxtdDSdEJNQ9lQO/Gg5JAn6EzeYgCvrZn4c1bsCSHq/r4u", null, null, null, false, null, false, null, "admin" });

            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { "user.registration", "User Registration" },
                    { "usergroup.registration", "User Group Registration" },
                    { "user.management", "User Management" },
                    { "usergroup.management", "User Group Management" }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumns: new[] { "Id", "ConcurrencyStamp" },
                keyValues: new object[] { "00000000-0000-0000-0000-000000000000", "5ff78f47-9190-4c1f-9ba5-bb3dd87c7a6f" });

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "user.management");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "user.registration");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "usergroup.management");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "usergroup.registration");
        }
    }
}
