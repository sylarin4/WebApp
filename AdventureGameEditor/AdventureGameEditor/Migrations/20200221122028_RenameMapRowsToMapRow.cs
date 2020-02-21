using Microsoft.EntityFrameworkCore.Migrations;

namespace AdventureGameEditor.Migrations
{
    public partial class RenameMapRowsToMapRow : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
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
                name: "MapRowID",
                table: "Field",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MapRow",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GameID = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MapRow", x => x.ID);
                    table.ForeignKey(
                        name: "FK_MapRow_Game_GameID",
                        column: x => x.GameID,
                        principalTable: "Game",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Field_MapRowID",
                table: "Field",
                column: "MapRowID");

            migrationBuilder.CreateIndex(
                name: "IX_MapRow_GameID",
                table: "MapRow",
                column: "GameID");

            migrationBuilder.AddForeignKey(
                name: "FK_Field_MapRow_MapRowID",
                table: "Field",
                column: "MapRowID",
                principalTable: "MapRow",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Field_MapRow_MapRowID",
                table: "Field");

            migrationBuilder.DropTable(
                name: "MapRow");

            migrationBuilder.DropIndex(
                name: "IX_Field_MapRowID",
                table: "Field");

            migrationBuilder.DropColumn(
                name: "MapRowID",
                table: "Field");

            migrationBuilder.AddColumn<int>(
                name: "MapRowsID",
                table: "Field",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MapRows",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GameID = table.Column<int>(type: "int", nullable: true)
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
    }
}
