using Microsoft.EntityFrameworkCore.Migrations;

namespace Identidade.Infraestrutura.Migrations
{
    public partial class FixingSubstituteUsers2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                name: "IX_UserSubstituteUser_SubstituteUserId",
                table: "UserSubstituteUser",
                column: "SubstituteUserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserSubstituteUser");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "00000000-0000-0000-0000-000000000000",
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "de781f02-46e9-4045-9568-de39755d1b29", "$2b$10$T2Gt7rGJu.8/4uexMHzF4ucLtCP6cmKsL1dXGbGY0qQyUa3wzag9q" });
        }
    }
}
