using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Identidade.Infraestrutura.Migrations
{
    public partial class RenamingUserSubstitution : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "00000000-0000-0000-0000-000000000000",
                columns: new[] { "ConcurrencyStamp", "LockoutEnd", "PasswordHash" },
                values: new object[] { "30a139ed-4579-47f3-965f-8ceef5c090a4", new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "$2b$10$PDjpm.JBK7IeUA7h5FVDquCIKzvjpXirKndof4Sj19dPlzQn9z/1." });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "00000000-0000-0000-0000-000000000000",
                columns: new[] { "ConcurrencyStamp", "LockoutEnd", "PasswordHash" },
                values: new object[] { "8ed8c73d-1ba9-4f0a-b198-db62d3dbd149", null, "$2b$10$UYVEcdAHTWdgpl6qZvpQVuWKP0iFBzshq8aMaZX0qYmeQoHBV5.k2" });
        }
    }
}
