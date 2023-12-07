using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebAPI.Migrations
{
    public partial class ModifyOwnerUsername : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "username",
                table: "Post",
                newName: "OwnerUsername");

            migrationBuilder.RenameColumn(
                name: "username",
                table: "Comment",
                newName: "OwnerUsername");

            migrationBuilder.RenameColumn(
                name: "username",
                table: "Blog",
                newName: "OwnerUsername");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OwnerUsername",
                table: "Post",
                newName: "username");

            migrationBuilder.RenameColumn(
                name: "OwnerUsername",
                table: "Comment",
                newName: "username");

            migrationBuilder.RenameColumn(
                name: "OwnerUsername",
                table: "Blog",
                newName: "username");
        }
    }
}
