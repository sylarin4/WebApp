using Microsoft.EntityFrameworkCore.Migrations;

namespace AdventureGameEditor.Migrations
{
    public partial class AddImageToGameResult : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ImageID",
                table: "GameResult",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_GameResult_ImageID",
                table: "GameResult",
                column: "ImageID");

            migrationBuilder.AddForeignKey(
                name: "FK_GameResult_Image_ImageID",
                table: "GameResult",
                column: "ImageID",
                principalTable: "Image",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GameResult_Image_ImageID",
                table: "GameResult");

            migrationBuilder.DropIndex(
                name: "IX_GameResult_ImageID",
                table: "GameResult");

            migrationBuilder.DropColumn(
                name: "ImageID",
                table: "GameResult");
        }
    }
}
