using Microsoft.EntityFrameworkCore.Migrations;

namespace Identidade.Infraestrutura.Migrations
{
    public partial class addingsftpcontrollerintegrationpermission : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "Identity",
                table: "Permissions",
                columns: new[] { "Id", "Name" },
                values: new object[] { "edita.exportacao.sftp.controller", null });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "Identity",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "edita.exportacao.sftp.controller");
        }
    }
}
