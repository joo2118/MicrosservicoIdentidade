using Microsoft.EntityFrameworkCore.Migrations;

namespace Identidade.Infraestrutura.Migrations
{
    public partial class UniqueGroupName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "UserGroups",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "USR_00000000-0000-0000-0000-000000000000",
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "71bae051-2891-4e3f-ba71-06408a27e64e", "$2b$10$RBJ09LHyio7wulaprcfgJeMX90tpuJvk0Qqw52QKwxm./snXII5CS" });

            migrationBuilder.CreateIndex(
                name: "IX_UserGroups_Name",
                table: "UserGroups",
                column: "Name",
                unique: true,
                filter: "[Name] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserGroups_Name",
                table: "UserGroups");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "UserGroups",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "USR_00000000-0000-0000-0000-000000000000",
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "1f5659b6-adde-4489-b081-38341c7f8793", "$2b$10$.BJ7hHn6aE0wecMe6DzuJe5br3s.NbDiRYiv/kyeKZt1WtSn8QSMG" });
        }
    }
}
