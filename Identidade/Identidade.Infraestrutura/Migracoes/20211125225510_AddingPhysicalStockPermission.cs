using Microsoft.EntityFrameworkCore.Migrations;

namespace Identidade.Infraestrutura.Migrations
{
    public partial class AddingPhysicalStockPermission : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "USR_00000000-0000-0000-0000-000000000000",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "0b995481-827a-45c6-961b-15eee48b9ab3", "$2b$10$z7hovysi2QSXbB92.LzNMubLwf9MExLfnhV.eG9QeF77g10HwwNIe", "30637bce-7787-4745-b08d-9a879c072bb0" });

            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "Name" },
                values: new object[] { "estoques.fisicos", null });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: "estoques.fisicos");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "USR_00000000-0000-0000-0000-000000000000",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "fea8fd8e-e1bf-4709-a193-b657ba56678c", "$2b$10$KKYv3oUSxO6msznn5cn7KuFM0KL7Jgdhb9YkGR9DddQAfzOQaTzTq", "b2482a87-9dfb-47c5-9737-423128835ac1" });
        }
    }
}
