using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Identidade.Infraestrutura.Migrations
{
    public partial class UserPasswordExpiration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PasswordMaturity",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "PasswordExpiration",
                table: "AspNetUsers",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "00000000-0000-0000-0000-000000000000",
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "6cf5a63d-baa8-4291-a44a-f6b3b1620457", "$2b$10$jBetrRUEmwQcYx2vq1BLyOlprEdCibJlKjfnxNr58Z7d1AJNrr7n6" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PasswordExpiration",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<DateTime>(
                name: "PasswordMaturity",
                table: "AspNetUsers",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "00000000-0000-0000-0000-000000000000",
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "5ff78f47-9190-4c1f-9ba5-bb3dd87c7a6f", "$2b$10$YJ7ApONRxtdDSdEJNQ9lQO/Gg5JAn6EzeYgCvrZn4c1bsCSHq/r4u" });
        }
    }
}
