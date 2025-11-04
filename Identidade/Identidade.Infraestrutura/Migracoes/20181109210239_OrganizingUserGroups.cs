using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Identidade.Infraestrutura.Migrations
{
    public partial class OrganizingUserGroups : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "UserGroups",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "LastUpdatedAt",
                table: "UserGroups",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "USR_00000000-0000-0000-0000-000000000000",
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "cc455035-c280-43a5-8715-44ff5e48a4eb", "$2b$10$I6J/6qgX3.3PNqVIY69R.OQKWGEyov.K85MFqTXIWmhmY1t2AzEOm" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "UserGroups");

            migrationBuilder.DropColumn(
                name: "LastUpdatedAt",
                table: "UserGroups");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "USR_00000000-0000-0000-0000-000000000000",
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "2019341b-7bd4-4882-bba0-e8e953e611e8", "$2b$10$zLkPfY65.EeLcsGkGVluOOendWqzS1.Q7Js5/5If1kVrpxLm1ccpi" });
        }
    }
}
