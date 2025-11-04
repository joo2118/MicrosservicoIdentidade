using Microsoft.EntityFrameworkCore.Migrations;

namespace Identidade.Infraestrutura.Migrations
{
    public partial class CreateTableSchemaIdentityArc : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Identity");

            migrationBuilder.RenameTable(
                name: "UserSubstitutions",
                newName: "UserSubstitutions",
                newSchema: "Identity");

            migrationBuilder.RenameTable(
                name: "UserGroupUser",
                newName: "UserGroupUser",
                newSchema: "Identity");

            migrationBuilder.RenameTable(
                name: "UserGroups",
                newName: "UserGroups",
                newSchema: "Identity");

            migrationBuilder.RenameTable(
                name: "UserGroupPermission",
                newName: "UserGroupPermission",
                newSchema: "Identity");

            migrationBuilder.RenameTable(
                name: "Permissions",
                newName: "Permissions",
                newSchema: "Identity");

            migrationBuilder.RenameTable(
                name: "ConsumedMessages",
                newName: "ConsumedMessages",
                newSchema: "Identity");

            migrationBuilder.RenameTable(
                name: "AspNetUserTokens",
                newName: "AspNetUserTokens",
                newSchema: "Identity");

            migrationBuilder.RenameTable(
                name: "AspNetUsers",
                newName: "AspNetUsers",
                newSchema: "Identity");

            migrationBuilder.RenameTable(
                name: "AspNetUserRoles",
                newName: "AspNetUserRoles",
                newSchema: "Identity");

            migrationBuilder.RenameTable(
                name: "AspNetUserLogins",
                newName: "AspNetUserLogins",
                newSchema: "Identity");

            migrationBuilder.RenameTable(
                name: "AspNetUserClaims",
                newName: "AspNetUserClaims",
                newSchema: "Identity");

            migrationBuilder.RenameTable(
                name: "AspNetRoles",
                newName: "AspNetRoles",
                newSchema: "Identity");

            migrationBuilder.RenameTable(
                name: "AspNetRoleClaims",
                newName: "AspNetRoleClaims",
                newSchema: "Identity");

            migrationBuilder.UpdateData(
                schema: "Identity",
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "USR_00000000-0000-0000-0000-000000000000",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "780267b9-331c-41f2-ab74-99524850a4ee", "$2b$10$C.3ydl12WYzf7IkLkoKlI.Dj8hMoZuaxZPybUeJy0LM5zDgL7tFV.", "dfa477d3-d4d3-4645-b236-b51c74e0b131" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "UserSubstitutions",
                schema: "Identity",
                newName: "UserSubstitutions");

            migrationBuilder.RenameTable(
                name: "UserGroupUser",
                schema: "Identity",
                newName: "UserGroupUser");

            migrationBuilder.RenameTable(
                name: "UserGroups",
                schema: "Identity",
                newName: "UserGroups");

            migrationBuilder.RenameTable(
                name: "UserGroupPermission",
                schema: "Identity",
                newName: "UserGroupPermission");

            migrationBuilder.RenameTable(
                name: "Permissions",
                schema: "Identity",
                newName: "Permissions");

            migrationBuilder.RenameTable(
                name: "ConsumedMessages",
                schema: "Identity",
                newName: "ConsumedMessages");

            migrationBuilder.RenameTable(
                name: "AspNetUserTokens",
                schema: "Identity",
                newName: "AspNetUserTokens");

            migrationBuilder.RenameTable(
                name: "AspNetUsers",
                schema: "Identity",
                newName: "AspNetUsers");

            migrationBuilder.RenameTable(
                name: "AspNetUserRoles",
                schema: "Identity",
                newName: "AspNetUserRoles");

            migrationBuilder.RenameTable(
                name: "AspNetUserLogins",
                schema: "Identity",
                newName: "AspNetUserLogins");

            migrationBuilder.RenameTable(
                name: "AspNetUserClaims",
                schema: "Identity",
                newName: "AspNetUserClaims");

            migrationBuilder.RenameTable(
                name: "AspNetRoles",
                schema: "Identity",
                newName: "AspNetRoles");

            migrationBuilder.RenameTable(
                name: "AspNetRoleClaims",
                schema: "Identity",
                newName: "AspNetRoleClaims");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "USR_00000000-0000-0000-0000-000000000000",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "aed4677b-7725-4eee-b3ae-680349afe3cf", "$2b$10$6pAMmmIj/QeARHSJhLxNbOBaP2VK80RRfTGvqqa5rHyg8u9WgSZNG", "0f8958ef-58fe-4726-b1a4-c63ea76123ec" });
        }
    }
}
