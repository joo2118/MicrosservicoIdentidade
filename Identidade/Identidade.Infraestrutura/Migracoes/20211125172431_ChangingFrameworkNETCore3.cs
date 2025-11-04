using Microsoft.EntityFrameworkCore.Migrations;

namespace Identidade.Infraestrutura.Migrations
{
    public partial class ChangingFrameworkNETCore3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
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
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "fea8fd8e-e1bf-4709-a193-b657ba56678c", "$2b$10$KKYv3oUSxO6msznn5cn7KuFM0KL7Jgdhb9YkGR9DddQAfzOQaTzTq", "b2482a87-9dfb-47c5-9737-423128835ac1" });

            migrationBuilder.AddForeignKey(
                name: "FK_UserSubstitutions_AspNetUsers_UserId",
                table: "UserSubstitutions",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserSubstitutions_AspNetUsers_UserId",
                table: "UserSubstitutions");

            migrationBuilder.AddColumn<string>(
                name: "UserId1",
                table: "UserSubstitutions",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "USR_00000000-0000-0000-0000-000000000000",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "09982c7b-5bea-4416-a60e-e3f13eb33de0", "$2b$10$P9itJzFZc0BkYrfuyN6pcejQf2Iw/ZyGOI.BOyVySXxiXl9CBYEhS", null });

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
    }
}
