using Microsoft.EntityFrameworkCore.Migrations;

namespace Identidade.Infraestrutura.Migrations
{
    public partial class FixingTableUsers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "00000000-0000-0000-0000-000000000000",
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "11f36524-dad8-4a4a-a0ea-dbe35998d339", "$2b$10$LE0XCco/UgPKIfvWWCaMUOrxiAe29Li5tvCFTUSayLWcEZXjmsoAC" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "00000000-0000-0000-0000-000000000000",
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "30a139ed-4579-47f3-965f-8ceef5c090a4", "$2b$10$PDjpm.JBK7IeUA7h5FVDquCIKzvjpXirKndof4Sj19dPlzQn9z/1." });
        }
    }
}
