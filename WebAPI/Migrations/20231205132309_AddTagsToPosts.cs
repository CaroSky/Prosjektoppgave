using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebAPI.Migrations
{
    public partial class AddTagsToPosts : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PostTag_Post_PostsPostId",
                table: "PostTag");

            migrationBuilder.DropForeignKey(
                name: "FK_PostTag_Tag_TagsTagId",
                table: "PostTag");

            migrationBuilder.DropIndex(
                name: "IX_PostTag_TagsTagId",
                table: "PostTag");

            migrationBuilder.UpdateData(
                table: "Post",
                keyColumn: "PostId",
                keyValue: 1,
                column: "Created",
                value: new DateTime(2023, 12, 5, 13, 23, 9, 170, DateTimeKind.Utc).AddTicks(5242));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Post",
                keyColumn: "PostId",
                keyValue: 1,
                column: "Created",
                value: new DateTime(2023, 12, 4, 13, 6, 14, 466, DateTimeKind.Utc).AddTicks(1249));

            migrationBuilder.CreateIndex(
                name: "IX_PostTag_TagsTagId",
                table: "PostTag",
                column: "TagsTagId");

            migrationBuilder.AddForeignKey(
                name: "FK_PostTag_Post_PostsPostId",
                table: "PostTag",
                column: "PostsPostId",
                principalTable: "Post",
                principalColumn: "PostId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PostTag_Tag_TagsTagId",
                table: "PostTag",
                column: "TagsTagId",
                principalTable: "Tag",
                principalColumn: "TagId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
