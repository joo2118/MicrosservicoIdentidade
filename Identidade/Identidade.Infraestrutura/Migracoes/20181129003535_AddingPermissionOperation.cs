using Microsoft.EntityFrameworkCore.Migrations;

namespace Identidade.Infraestrutura.Migrations
{
    public partial class AddingPermissionOperation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PermissionOperation",
                table: "UserGroupPermission",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "USR_00000000-0000-0000-0000-000000000000",
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "f7817e22-1cae-4981-9c0c-b399d3031b56", "$2b$10$onrN4P8WsMYejcFuUA7.3OcqoR60WsuDF5y3kanlxmYGhTl3evj/q" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PermissionOperation",
                table: "UserGroupPermission");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "USR_00000000-0000-0000-0000-000000000000",
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "b87ca104-abfe-4706-a695-0ffe60c9fd26", "$2b$10$LGcMO9lqgqtep7ZBGrUyiupRXUc3fQZBL8VGzFAkbZGORVLnXYsMC" });
        }
    }
}
