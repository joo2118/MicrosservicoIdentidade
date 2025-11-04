using Microsoft.EntityFrameworkCore.Migrations;

namespace Identidade.Infraestrutura.Migrations
{
    public partial class FixingLinkingTreatmentsOnPermissions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserGroups_Permissions_PermissionId",
                table: "UserGroups");

            migrationBuilder.DropIndex(
                name: "IX_UserGroups_PermissionId",
                table: "UserGroups");

            migrationBuilder.DropColumn(
                name: "PermissionId",
                table: "UserGroups");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "USR_00000000-0000-0000-0000-000000000000",
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "b87ca104-abfe-4706-a695-0ffe60c9fd26", "$2b$10$LGcMO9lqgqtep7ZBGrUyiupRXUc3fQZBL8VGzFAkbZGORVLnXYsMC" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PermissionId",
                table: "UserGroups",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "USR_00000000-0000-0000-0000-000000000000",
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "3248006a-e607-46e4-b685-8b690ee6d6e6", "$2b$10$xritsoc.CbQ9rgqKJ0nuBObK2iF6NuxNj4ENQ/fep/.6pc3xpYGlO" });

            migrationBuilder.CreateIndex(
                name: "IX_UserGroups_PermissionId",
                table: "UserGroups",
                column: "PermissionId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserGroups_Permissions_PermissionId",
                table: "UserGroups",
                column: "PermissionId",
                principalTable: "Permissions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
