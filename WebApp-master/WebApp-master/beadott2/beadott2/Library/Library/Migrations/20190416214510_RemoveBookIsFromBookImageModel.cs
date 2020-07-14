using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Library.Migrations
{
    public partial class RemoveBookIsFromBookImageModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BookImage_Book_BookId",
                table: "BookImage");

            migrationBuilder.RenameColumn(
                name: "BookId",
                table: "BookImage",
                newName: "BookID");

            migrationBuilder.RenameIndex(
                name: "IX_BookImage_BookId",
                table: "BookImage",
                newName: "IX_BookImage_BookID");

            migrationBuilder.AlterColumn<int>(
                name: "BookID",
                table: "BookImage",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AddForeignKey(
                name: "FK_BookImage_Book_BookID",
                table: "BookImage",
                column: "BookID",
                principalTable: "Book",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BookImage_Book_BookID",
                table: "BookImage");

            migrationBuilder.RenameColumn(
                name: "BookID",
                table: "BookImage",
                newName: "BookId");

            migrationBuilder.RenameIndex(
                name: "IX_BookImage_BookID",
                table: "BookImage",
                newName: "IX_BookImage_BookId");

            migrationBuilder.AlterColumn<int>(
                name: "BookId",
                table: "BookImage",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_BookImage_Book_BookId",
                table: "BookImage",
                column: "BookId",
                principalTable: "Book",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
