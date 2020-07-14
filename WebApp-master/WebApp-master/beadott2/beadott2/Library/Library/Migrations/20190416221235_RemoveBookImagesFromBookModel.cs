using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Library.Migrations
{
    public partial class RemoveBookImagesFromBookModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BookImage_Book_BookID",
                table: "BookImage");

            migrationBuilder.DropIndex(
                name: "IX_BookImage_BookID",
                table: "BookImage");

            migrationBuilder.DropColumn(
                name: "BookID",
                table: "BookImage");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BookID",
                table: "BookImage",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BookImage_BookID",
                table: "BookImage",
                column: "BookID");

            migrationBuilder.AddForeignKey(
                name: "FK_BookImage_Book_BookID",
                table: "BookImage",
                column: "BookID",
                principalTable: "Book",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
