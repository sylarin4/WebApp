using Microsoft.EntityFrameworkCore.Migrations;

namespace AdventureGameEditor.Migrations
{
    public partial class AddGameResult : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GameLostResultID",
                table: "Game",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GameWonResultID",
                table: "Game",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "GameResult",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OwnerId = table.Column<int>(nullable: true),
                    GameTitle = table.Column<string>(nullable: true),
                    IsGameWonResult = table.Column<bool>(nullable: false),
                    Text = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameResult", x => x.ID);
                    table.ForeignKey(
                        name: "FK_GameResult_User_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Game_GameLostResultID",
                table: "Game",
                column: "GameLostResultID");

            migrationBuilder.CreateIndex(
                name: "IX_Game_GameWonResultID",
                table: "Game",
                column: "GameWonResultID");

            migrationBuilder.CreateIndex(
                name: "IX_GameResult_OwnerId",
                table: "GameResult",
                column: "OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Game_GameResult_GameLostResultID",
                table: "Game",
                column: "GameLostResultID",
                principalTable: "GameResult",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Game_GameResult_GameWonResultID",
                table: "Game",
                column: "GameWonResultID",
                principalTable: "GameResult",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Game_GameResult_GameLostResultID",
                table: "Game");

            migrationBuilder.DropForeignKey(
                name: "FK_Game_GameResult_GameWonResultID",
                table: "Game");

            migrationBuilder.DropTable(
                name: "GameResult");

            migrationBuilder.DropIndex(
                name: "IX_Game_GameLostResultID",
                table: "Game");

            migrationBuilder.DropIndex(
                name: "IX_Game_GameWonResultID",
                table: "Game");

            migrationBuilder.DropColumn(
                name: "GameLostResultID",
                table: "Game");

            migrationBuilder.DropColumn(
                name: "GameWonResultID",
                table: "Game");
        }
    }
}
