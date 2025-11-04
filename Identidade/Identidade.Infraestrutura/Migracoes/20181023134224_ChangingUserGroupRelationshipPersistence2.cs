using Microsoft.EntityFrameworkCore.Migrations;

namespace Identidade.Infraestrutura.Migrations
{
    public partial class ChangingUserGroupRelationshipPersistence2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "00000000-0000-0000-0000-000000000000",
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "8ed8c73d-1ba9-4f0a-b198-db62d3dbd149", "$2b$10$UYVEcdAHTWdgpl6qZvpQVuWKP0iFBzshq8aMaZX0qYmeQoHBV5.k2" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "00000000-0000-0000-0000-000000000000",
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "1c97f2d6-9b1c-416a-8af0-2f9d392a8c79", "$2b$10$wQ487d04okv66kN.9Qjl/u6HgOAOWmyn5hTSO/p3888Swp4bMpbCi" });
        }
    }
}
