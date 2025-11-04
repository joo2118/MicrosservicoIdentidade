using Microsoft.EntityFrameworkCore.Migrations;

namespace Identidade.Infraestrutura.Migrations
{
    public partial class NovapermissãotelaConciliaçãoPU : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "Identity",
                table: "Permissions",
                columns: new[] { "Id", "Name" },
                values: new object[] { "conciliacao.pu.operacao", null });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "Identity",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "conciliacao.pu.operacao");
        }
    }
}
