using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Identidade.Infraestrutura.Migrations
{
    public partial class RemoveAdminUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "Identity",
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "USR_00000000-0000-0000-0000-000000000000");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "Identity",
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "AuthenticationType", "ConcurrencyStamp", "CreatedAt", "Email", "EmailConfirmed", "Language", "LastUpdatedAt", "LockoutEnabled", "LockoutEnd", "Name", "NormalizedEmail", "NormalizedUserName", "PasswordDoesNotExpire", "PasswordExpiration", "PasswordHash", "PasswordHistory", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { "USR_00000000-0000-0000-0000-000000000000", 0, null, "780267b9-331c-41f2-ab74-99524850a4ee", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, false, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Administrator", null, null, null, null, "$2b$10$C.3ydl12WYzf7IkLkoKlI.Dj8hMoZuaxZPybUeJy0LM5zDgL7tFV.", "", null, false, "dfa477d3-d4d3-4645-b236-b51c74e0b131", false, "admin" });
        }
    }
}
