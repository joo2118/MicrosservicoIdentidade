using Microsoft.EntityFrameworkCore.Migrations;

namespace Identidade.Infraestrutura.Migrations
{
    public partial class InsertingMessageId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConsumedMessages",
                columns: table => new
                {
                    MessageId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsumedMessages", x => x.MessageId);
                });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "USR_00000000-0000-0000-0000-000000000000",
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "3a261500-3c23-4ab7-b3e6-3f7c38b46de6", "$2b$10$imeGHlJtQHmdko97W6DDIeVn2et9n8SCVN3iQVIviW.EBGa57cT4q" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConsumedMessages");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "USR_00000000-0000-0000-0000-000000000000",
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "a8d630bb-25ae-4527-9f4b-b60dd1055b4b", "$2b$10$5KLD7QESIcHEh29oLZi3L.3hJUTXehvqmxGstyKbl4w0gg7jOh40e" });
        }
    }
}
