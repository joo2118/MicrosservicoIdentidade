using Microsoft.EntityFrameworkCore.Migrations;

namespace Identidade.Infraestrutura.Migrations
{
    public partial class FixingPermissionOperations : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PermissionOperation",
                table: "UserGroupPermission",
                newName: "PermissionOperations");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "USR_00000000-0000-0000-0000-000000000000",
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "a8d630bb-25ae-4527-9f4b-b60dd1055b4b", "$2b$10$5KLD7QESIcHEh29oLZi3L.3hJUTXehvqmxGstyKbl4w0gg7jOh40e" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PermissionOperations",
                table: "UserGroupPermission",
                newName: "PermissionOperation");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "USR_00000000-0000-0000-0000-000000000000",
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "f7817e22-1cae-4981-9c0c-b399d3031b56", "$2b$10$onrN4P8WsMYejcFuUA7.3OcqoR60WsuDF5y3kanlxmYGhTl3evj/q" });
        }
    }
}
