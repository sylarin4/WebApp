using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AdventureGameEditor.Migrations
{
    public partial class ChangeImageInPrelude : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Image",
                table: "Prelude");

            migrationBuilder.AddColumn<int>(
                name: "ImageID",
                table: "Prelude",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Image",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(nullable: true),
                    Picture = table.Column<byte[]>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Image", x => x.ID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Prelude_ImageID",
                table: "Prelude",
                column: "ImageID");

            migrationBuilder.AddForeignKey(
                name: "FK_Prelude_Image_ImageID",
                table: "Prelude",
                column: "ImageID",
                principalTable: "Image",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Prelude_Image_ImageID",
                table: "Prelude");

            migrationBuilder.DropTable(
                name: "Image");

            migrationBuilder.DropIndex(
                name: "IX_Prelude_ImageID",
                table: "Prelude");

            migrationBuilder.DropColumn(
                name: "ImageID",
                table: "Prelude");

            migrationBuilder.AddColumn<byte[]>(
                name: "Image",
                table: "Prelude",
                type: "varbinary(max)",
                nullable: true);
        }
    }
}
