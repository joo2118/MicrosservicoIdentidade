using Microsoft.EntityFrameworkCore.Migrations;

namespace Identidade.Infraestrutura.Migrations
{
    public partial class ChangingUserGroupRelationshipPersistence : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserGroupPermission");

            migrationBuilder.DropTable(
                name: "UserGroupUser");

            migrationBuilder.DropTable(
                name: "UserSubstituteUser");

            migrationBuilder.AddColumn<string>(
                name: "PermissionId",
                table: "UserGroups",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "UserGroups",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserGroupId",
                table: "Permissions",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "UserSubstitutions",
                columns: table => new
                {
                    UserId = table.Column<string>(nullable: false),
                    SubstituteUserId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSubstitutions", x => new { x.UserId, x.SubstituteUserId });
                    table.ForeignKey(
                        name: "FK_UserSubstitutions_AspNetUsers_SubstituteUserId",
                        column: x => x.SubstituteUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserSubstitutions_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "00000000-0000-0000-0000-000000000000",
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "1c97f2d6-9b1c-416a-8af0-2f9d392a8c79", "$2b$10$wQ487d04okv66kN.9Qjl/u6HgOAOWmyn5hTSO/p3888Swp4bMpbCi" });

            migrationBuilder.CreateIndex(
                name: "IX_UserGroups_PermissionId",
                table: "UserGroups",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_UserGroups_UserId",
                table: "UserGroups",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_UserGroupId",
                table: "Permissions",
                column: "UserGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_UserSubstitutions_SubstituteUserId",
                table: "UserSubstitutions",
                column: "SubstituteUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Permissions_UserGroups_UserGroupId",
                table: "Permissions",
                column: "UserGroupId",
                principalTable: "UserGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserGroups_Permissions_PermissionId",
                table: "UserGroups",
                column: "PermissionId",
                principalTable: "Permissions",
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

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Permissions_UserGroups_UserGroupId",
                table: "Permissions");

            migrationBuilder.DropForeignKey(
                name: "FK_UserGroups_Permissions_PermissionId",
                table: "UserGroups");

            migrationBuilder.DropForeignKey(
                name: "FK_UserGroups_AspNetUsers_UserId",
                table: "UserGroups");

            migrationBuilder.DropTable(
                name: "UserSubstitutions");

            migrationBuilder.DropIndex(
                name: "IX_UserGroups_PermissionId",
                table: "UserGroups");

            migrationBuilder.DropIndex(
                name: "IX_UserGroups_UserId",
                table: "UserGroups");

            migrationBuilder.DropIndex(
                name: "IX_Permissions_UserGroupId",
                table: "Permissions");

            migrationBuilder.DropColumn(
                name: "PermissionId",
                table: "UserGroups");

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

            migrationBuilder.CreateTable(
                name: "UserSubstituteUser",
                columns: table => new
                {
                    UserId = table.Column<string>(nullable: false),
                    SubstituteUserId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSubstituteUser", x => new { x.UserId, x.SubstituteUserId });
                    table.ForeignKey(
                        name: "FK_UserSubstituteUser_AspNetUsers_SubstituteUserId",
                        column: x => x.SubstituteUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "00000000-0000-0000-0000-000000000000",
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "33cd4511-ceba-462f-aa48-3e140958a13e", "$2b$10$UgT3sL1wiZCW.geUsDFmYe/CSSMg5ZW9RZfVaWZGKg4h5.eyBNTjG" });

            migrationBuilder.CreateIndex(
                name: "IX_UserGroupPermission_PermissionId",
                table: "UserGroupPermission",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_UserGroupUser_UserId",
                table: "UserGroupUser",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserSubstituteUser_SubstituteUserId",
                table: "UserSubstituteUser",
                column: "SubstituteUserId");
        }
    }
}
