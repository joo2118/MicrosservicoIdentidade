using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Identidade.Infraestrutura.Migrations
{
    public partial class FixingAdminUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumns: new[] { "Id", "ConcurrencyStamp" },
                keyValues: new object[] { "00000000-0000-0000-0000-000000000000", "11f36524-dad8-4a4a-a0ea-dbe35998d339" });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "CreatedAt", "Email", "EmailConfirmed", "LastUpdatedAt", "LockoutEnabled", "LockoutEnd", "Name", "NormalizedEmail", "NormalizedUserName", "PasswordDoesNotExpire", "PasswordExpiration", "PasswordHash", "PasswordHistory", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { "USR_00000000-0000-0000-0000-000000000000", 0, "2019341b-7bd4-4882-bba0-e8e953e611e8", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, false, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Administrator", null, null, null, null, "$2b$10$zLkPfY65.EeLcsGkGVluOOendWqzS1.Q7Js5/5If1kVrpxLm1ccpi", "", null, false, null, false, "admin" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumns: new[] { "Id", "ConcurrencyStamp" },
                keyValues: new object[] { "USR_00000000-0000-0000-0000-000000000000", "2019341b-7bd4-4882-bba0-e8e953e611e8" });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "CreatedAt", "Email", "EmailConfirmed", "LastUpdatedAt", "LockoutEnabled", "LockoutEnd", "Name", "NormalizedEmail", "NormalizedUserName", "PasswordDoesNotExpire", "PasswordExpiration", "PasswordHash", "PasswordHistory", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { "00000000-0000-0000-0000-000000000000", 0, "11f36524-dad8-4a4a-a0ea-dbe35998d339", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, false, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Administrator", null, null, null, null, "$2b$10$LE0XCco/UgPKIfvWWCaMUOrxiAe29Li5tvCFTUSayLWcEZXjmsoAC", null, null, false, null, false, "admin" });
        }
    }
}
