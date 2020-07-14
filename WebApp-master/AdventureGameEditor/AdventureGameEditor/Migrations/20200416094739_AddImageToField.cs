using Microsoft.EntityFrameworkCore.Migrations;

namespace AdventureGameEditor.Migrations
{
    public partial class AddImageToField : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ImageID",
                table: "Field",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Field_ImageID",
                table: "Field",
                column: "ImageID");

            migrationBuilder.AddForeignKey(
                name: "FK_Field_Image_ImageID",
                table: "Field",
                column: "ImageID",
                principalTable: "Image",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Field_Image_ImageID",
                table: "Field");

            migrationBuilder.DropIndex(
                name: "IX_Field_ImageID",
                table: "Field");

            migrationBuilder.DropColumn(
                name: "ImageID",
                table: "Field");
        }
    }
}
