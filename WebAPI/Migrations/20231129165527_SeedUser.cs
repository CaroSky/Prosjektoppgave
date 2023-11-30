using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebAPI.Migrations
{
    public partial class SeedUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "38615521-3ed4-43e1-afc5-415e94cb4f3e");

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "Email", "EmailConfirmed", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { "0cde7c88-d719-4905-89f4-9b94ce2390c2", 0, "888a17a2-9911-40ee-b4f6-e4094d9189ef", "testuser@example.com", true, false, null, "TESTUSER@EXAMPLE.COM", "TESTUSER", "AQAAAAEAACcQAAAAEDcsgHd+J1R5HEX1Imw+2scjWIWRq4kyN48+8K1PxdKjAtFzUFSiUrsmxbMZpAyiFw==", null, false, "", false, "testuser" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "0cde7c88-d719-4905-89f4-9b94ce2390c2");

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "Email", "EmailConfirmed", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { "38615521-3ed4-43e1-afc5-415e94cb4f3e", 0, "7752cbbb-94fb-47a6-ab99-34f462a8058c", "testuser@example.com", true, false, null, "TESTUSER@EXAMPLE.COM", "TESTUSER", "AQAAAAEAACcQAAAAEKBkssEZpS0l5GGk+y7WGYAxfh2QJ+/rp8HeVYvANwvxlVAmv4SrKkvTEC2bnjrBHg==", null, false, "", false, "testuser" });
        }
    }
}
