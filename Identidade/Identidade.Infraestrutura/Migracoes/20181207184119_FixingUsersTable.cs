using Microsoft.EntityFrameworkCore.Migrations;

namespace Identidade.Infraestrutura.Migrations
{
    public partial class FixingUsersTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserSubstitutions_AspNetUsers_UserId",
                table: "UserSubstitutions");

            migrationBuilder.AddColumn<string>(
                name: "UserId1",
                table: "UserSubstitutions",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "USR_00000000-0000-0000-0000-000000000000",
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "c4f440fa-b55c-4c9a-8f74-db7fe5863f7d", "$2b$10$.U2QitCCZxeTH2W7JX0eH.OgoSCz1dk6WIVjly.k59VvO5gv/7ffq" });

            migrationBuilder.CreateIndex(
                name: "IX_UserSubstitutions_UserId1",
                table: "UserSubstitutions",
                column: "UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_UserSubstitutions_AspNetUsers_UserId1",
                table: "UserSubstitutions",
                column: "UserId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserSubstitutions_AspNetUsers_UserId1",
                table: "UserSubstitutions");

            migrationBuilder.DropIndex(
                name: "IX_UserSubstitutions_UserId1",
                table: "UserSubstitutions");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "UserSubstitutions");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "USR_00000000-0000-0000-0000-000000000000",
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "71bae051-2891-4e3f-ba71-06408a27e64e", "$2b$10$RBJ09LHyio7wulaprcfgJeMX90tpuJvk0Qqw52QKwxm./snXII5CS" });

            migrationBuilder.AddForeignKey(
                name: "FK_UserSubstitutions_AspNetUsers_UserId",
                table: "UserSubstitutions",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
