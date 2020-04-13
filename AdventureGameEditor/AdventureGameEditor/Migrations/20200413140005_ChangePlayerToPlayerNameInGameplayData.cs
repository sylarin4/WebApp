using Microsoft.EntityFrameworkCore.Migrations;

namespace AdventureGameEditor.Migrations
{
    public partial class ChangePlayerToPlayerNameInGameplayData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GameplayData_User_PlayerId",
                table: "GameplayData");

            migrationBuilder.DropIndex(
                name: "IX_GameplayData_PlayerId",
                table: "GameplayData");

            migrationBuilder.DropColumn(
                name: "PlayerId",
                table: "GameplayData");

            migrationBuilder.AddColumn<string>(
                name: "PlayerName",
                table: "GameplayData",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PlayerName",
                table: "GameplayData");

            migrationBuilder.AddColumn<int>(
                name: "PlayerId",
                table: "GameplayData",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_GameplayData_PlayerId",
                table: "GameplayData",
                column: "PlayerId");

            migrationBuilder.AddForeignKey(
                name: "FK_GameplayData_User_PlayerId",
                table: "GameplayData",
                column: "PlayerId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
