using Microsoft.EntityFrameworkCore.Migrations;

namespace AdventureGameEditor.Migrations
{
    public partial class AddGameConditionToGameplayDataInsteadOfIsGameOver : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsGameOver",
                table: "GameplayData");

            migrationBuilder.AddColumn<int>(
                name: "GameCondition",
                table: "GameplayData",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GameCondition",
                table: "GameplayData");

            migrationBuilder.AddColumn<bool>(
                name: "IsGameOver",
                table: "GameplayData",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
