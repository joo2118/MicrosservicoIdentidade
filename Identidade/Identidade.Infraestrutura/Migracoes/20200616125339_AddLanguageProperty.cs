using Microsoft.EntityFrameworkCore.Migrations;

namespace Identidade.Infraestrutura.Migrations
{
    public partial class AddLanguageProperty : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Language",
                table: "AspNetUsers",
                nullable: true);

            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { "visualizacao.menu.integracoes.externas", null },
                    { "finalizar.criacao.contratos", null },
                    { "expirar.senha.usuario", null },
                    { "cancelamento.eventos", null },
                    { "boletagem.alcada.validacao.NiveisValidacao.6", null },
                    { "boletagem.alcada.validacao.NiveisValidacao.5", null },
                    { "boletagem.alcada.validacao.NiveisValidacao.4", null },
                    { "boletagem.alcada.validacao.NiveisValidacao.3", null },
                    { "boletagem.alcada.validacao.NiveisValidacao.2", null },
                    { "boletagem.alcada.validacao.NiveisValidacao.1", null },
                    { "boletagem.alteracao.boletas.validadas.NiveisValidacao.6", null },
                    { "boletagem.alteracao.boletas.validadas.NiveisValidacao.5", null },
                    { "boletagem.alteracao.boletas.validadas.NiveisValidacao.4", null },
                    { "boletagem.alteracao.boletas.validadas.NiveisValidacao.3", null },
                    { "boletagem.alteracao.boletas.validadas.NiveisValidacao.2", null },
                    { "visualizacao.ferramentas.customizadas.externas", null },
                    { "boletagem.alteracao.boletas.validadas.NiveisValidacao.1", null }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "boletagem.alcada.validacao.NiveisValidacao.1");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "boletagem.alcada.validacao.NiveisValidacao.2");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "boletagem.alcada.validacao.NiveisValidacao.3");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "boletagem.alcada.validacao.NiveisValidacao.4");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "boletagem.alcada.validacao.NiveisValidacao.5");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "boletagem.alcada.validacao.NiveisValidacao.6");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "boletagem.alteracao.boletas.validadas.NiveisValidacao.1");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "boletagem.alteracao.boletas.validadas.NiveisValidacao.2");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "boletagem.alteracao.boletas.validadas.NiveisValidacao.3");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "boletagem.alteracao.boletas.validadas.NiveisValidacao.4");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "boletagem.alteracao.boletas.validadas.NiveisValidacao.5");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "boletagem.alteracao.boletas.validadas.NiveisValidacao.6");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "cancelamento.eventos");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "expirar.senha.usuario");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "finalizar.criacao.contratos");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "visualizacao.ferramentas.customizadas.externas");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "visualizacao.menu.integracoes.externas");

            migrationBuilder.DropColumn(
                name: "Language",
                table: "AspNetUsers");
        }
    }
}
