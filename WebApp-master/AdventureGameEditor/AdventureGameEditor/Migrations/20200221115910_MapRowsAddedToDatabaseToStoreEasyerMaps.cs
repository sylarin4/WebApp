using Microsoft.EntityFrameworkCore.Migrations;

namespace AdventureGameEditor.Migrations
{
    public partial class MapRowsAddedToDatabaseToStoreEasyerMaps : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Field_Game_GameID",
                table: "Field");

            migrationBuilder.DropIndex(
                name: "IX_Field_GameID",
                table: "Field");

            migrationBuilder.DropColumn(
                name: "GameID",
                table: "Field");

            migrationBuilder.AddColumn<int>(
                name: "MapRowsID",
                table: "Field",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MapRows",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GameID = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MapRows", x => x.ID);
                    table.ForeignKey(
                        name: "FK_MapRows_Game_GameID",
                        column: x => x.GameID,
                        principalTable: "Game",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Field_MapRowsID",
                table: "Field",
                column: "MapRowsID");

            migrationBuilder.CreateIndex(
                name: "IX_MapRows_GameID",
                table: "MapRows",
                column: "GameID");

            migrationBuilder.AddForeignKey(
                name: "FK_Field_MapRows_MapRowsID",
                table: "Field",
                column: "MapRowsID",
                principalTable: "MapRows",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Field_MapRows_MapRowsID",
                table: "Field");

            migrationBuilder.DropTable(
                name: "MapRows");

            migrationBuilder.DropIndex(
                name: "IX_Field_MapRowsID",
                table: "Field");

            migrationBuilder.DropColumn(
                name: "MapRowsID",
                table: "Field");

            migrationBuilder.AddColumn<int>(
                name: "GameID",
                table: "Field",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Field_GameID",
                table: "Field",
                column: "GameID");

            migrationBuilder.AddForeignKey(
                name: "FK_Field_Game_GameID",
                table: "Field",
                column: "GameID",
                principalTable: "Game",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
