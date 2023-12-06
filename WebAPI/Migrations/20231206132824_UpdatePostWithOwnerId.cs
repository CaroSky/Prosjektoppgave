using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebAPI.Migrations
{
    public partial class UpdatePostWithOwnerId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Post",
                keyColumn: "PostId",
                keyValue: 1);

            migrationBuilder.AddColumn<string>(
                name: "OwnerId",
                table: "Post",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Post");

            migrationBuilder.InsertData(
                table: "Post",
                columns: new[] { "PostId", "BlogId", "Content", "Created", "IsCommentAllowed", "Title" },
                values: new object[] { 1, 1006, "Dette er innholdet i den seedede posten.", new DateTime(2023, 12, 5, 19, 43, 42, 444, DateTimeKind.Utc).AddTicks(6074), true, "Seedet Post Tittel" });
        }
    }
}
