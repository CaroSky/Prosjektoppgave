using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebAPI.Migrations
{
    public partial class ChangeImageFormat : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageData",
                table: "Post");

            migrationBuilder.RenameColumn(
                name: "ImageFileName",
                table: "Post",
                newName: "ImageBase64");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ImageBase64",
                table: "Post",
                newName: "ImageFileName");

            migrationBuilder.AddColumn<byte[]>(
                name: "ImageData",
                table: "Post",
                type: "varbinary(max)",
                nullable: false,
                defaultValue: new byte[0]);
        }
    }
}
