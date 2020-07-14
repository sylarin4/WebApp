using Microsoft.EntityFrameworkCore.Migrations;

namespace AdventureGameEditor.Migrations
{
    public partial class OwnerAddedToGameModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExitRoads",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IsRightWay = table.Column<bool>(nullable: false),
                    IsLeftWay = table.Column<bool>(nullable: false),
                    IsUpWay = table.Column<bool>(nullable: false),
                    IsDownWay = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExitRoads", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "FieldCoordinate",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HorizontalCoordinate = table.Column<int>(nullable: false),
                    VerticalCoordinate = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FieldCoordinate", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Field",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FieldCoordinateID = table.Column<int>(nullable: true),
                    Text = table.Column<string>(nullable: true),
                    TrialID = table.Column<int>(nullable: true),
                    ExitRoadsID = table.Column<int>(nullable: true),
                    GameID = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Field", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Field_ExitRoads_ExitRoadsID",
                        column: x => x.ExitRoadsID,
                        principalTable: "ExitRoads",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Field_FieldCoordinate_FieldCoordinateID",
                        column: x => x.FieldCoordinateID,
                        principalTable: "FieldCoordinate",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Game",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OwnerId = table.Column<int>(nullable: true),
                    Title = table.Column<string>(nullable: true),
                    Visibility = table.Column<int>(nullable: false),
                    PlayCounter = table.Column<int>(nullable: false),
                    TableSize = table.Column<int>(nullable: false),
                    StartFieldID = table.Column<int>(nullable: true),
                    TargetFieldID = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Game", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Game_User_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Game_Field_StartFieldID",
                        column: x => x.StartFieldID,
                        principalTable: "Field",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Game_Field_TargetFieldID",
                        column: x => x.TargetFieldID,
                        principalTable: "Field",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Field_ExitRoadsID",
                table: "Field",
                column: "ExitRoadsID");

            migrationBuilder.CreateIndex(
                name: "IX_Field_FieldCoordinateID",
                table: "Field",
                column: "FieldCoordinateID");

            migrationBuilder.CreateIndex(
                name: "IX_Field_GameID",
                table: "Field",
                column: "GameID");

            migrationBuilder.CreateIndex(
                name: "IX_Game_OwnerId",
                table: "Game",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Game_StartFieldID",
                table: "Game",
                column: "StartFieldID");

            migrationBuilder.CreateIndex(
                name: "IX_Game_TargetFieldID",
                table: "Game",
                column: "TargetFieldID");

            migrationBuilder.AddForeignKey(
                name: "FK_Field_Game_GameID",
                table: "Field",
                column: "GameID",
                principalTable: "Game",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Field_ExitRoads_ExitRoadsID",
                table: "Field");

            migrationBuilder.DropForeignKey(
                name: "FK_Field_FieldCoordinate_FieldCoordinateID",
                table: "Field");

            migrationBuilder.DropForeignKey(
                name: "FK_Field_Game_GameID",
                table: "Field");

            migrationBuilder.DropTable(
                name: "ExitRoads");

            migrationBuilder.DropTable(
                name: "FieldCoordinate");

            migrationBuilder.DropTable(
                name: "Game");

            migrationBuilder.DropTable(
                name: "Field");
        }
    }
}
