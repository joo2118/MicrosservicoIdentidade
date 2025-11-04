using Microsoft.EntityFrameworkCore.Migrations;

namespace Identidade.Infraestrutura.Migrations
{
    public partial class RevertingLinkingTreatments : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Permissions_UserGroups_UserGroupId",
                table: "Permissions");

            migrationBuilder.DropForeignKey(
                name: "FK_UserGroups_AspNetUsers_UserId",
                table: "UserGroups");

            migrationBuilder.DropIndex(
                name: "IX_UserGroups_UserId",
                table: "UserGroups");

            migrationBuilder.DropIndex(
                name: "IX_Permissions_UserGroupId",
                table: "Permissions");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "UserGroups");

            migrationBuilder.DropColumn(
                name: "UserGroupId",
                table: "Permissions");

            migrationBuilder.CreateTable(
                name: "UserGroupPermission",
                columns: table => new
                {
                    UserGroupId = table.Column<string>(nullable: false),
                    PermissionId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserGroupPermission", x => new { x.UserGroupId, x.PermissionId });
                    table.ForeignKey(
                        name: "FK_UserGroupPermission_Permissions_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "Permissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserGroupPermission_UserGroups_UserGroupId",
                        column: x => x.UserGroupId,
                        principalTable: "UserGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserGroupUser",
                columns: table => new
                {
                    UserGroupId = table.Column<string>(nullable: false),
                    UserId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserGroupUser", x => new { x.UserGroupId, x.UserId });
                    table.ForeignKey(
                        name: "FK_UserGroupUser_UserGroups_UserGroupId",
                        column: x => x.UserGroupId,
                        principalTable: "UserGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserGroupUser_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "USR_00000000-0000-0000-0000-000000000000",
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "3248006a-e607-46e4-b685-8b690ee6d6e6", "$2b$10$xritsoc.CbQ9rgqKJ0nuBObK2iF6NuxNj4ENQ/fep/.6pc3xpYGlO" });

            migrationBuilder.CreateIndex(
                name: "IX_UserGroupPermission_PermissionId",
                table: "UserGroupPermission",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_UserGroupUser_UserId",
                table: "UserGroupUser",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserGroupPermission");

            migrationBuilder.DropTable(
                name: "UserGroupUser");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "UserGroups",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserGroupId",
                table: "Permissions",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "USR_00000000-0000-0000-0000-000000000000",
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "831ba00e-e58c-440d-97a6-f7e2f8821694", "$2b$10$jgKX8IulAbktaIsuqG9Etu4yFxcLU7pwc4T3HuTUeQyddhYqcBgvm" });

            migrationBuilder.CreateIndex(
                name: "IX_UserGroups_UserId",
                table: "UserGroups",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_UserGroupId",
                table: "Permissions",
                column: "UserGroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_Permissions_UserGroups_UserGroupId",
                table: "Permissions",
                column: "UserGroupId",
                principalTable: "UserGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserGroups_AspNetUsers_UserId",
                table: "UserGroups",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
