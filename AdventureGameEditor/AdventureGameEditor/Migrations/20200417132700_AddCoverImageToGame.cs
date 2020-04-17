using Microsoft.EntityFrameworkCore.Migrations;

namespace AdventureGameEditor.Migrations
{
    public partial class AddCoverImageToGame : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CoverImageID",
                table: "Game",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Game_CoverImageID",
                table: "Game",
                column: "CoverImageID");

            migrationBuilder.AddForeignKey(
                name: "FK_Game_Image_CoverImageID",
                table: "Game",
                column: "CoverImageID",
                principalTable: "Image",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Game_Image_CoverImageID",
                table: "Game");

            migrationBuilder.DropIndex(
                name: "IX_Game_CoverImageID",
                table: "Game");

            migrationBuilder.DropColumn(
                name: "CoverImageID",
                table: "Game");
        }
    }
}
