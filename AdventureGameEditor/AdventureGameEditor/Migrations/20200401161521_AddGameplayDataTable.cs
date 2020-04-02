using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AdventureGameEditor.Migrations
{
    public partial class AddGameplayDataTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GameplayData",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlayerId = table.Column<int>(nullable: true),
                    CurrentPlayerPositionID = table.Column<int>(nullable: true),
                    StepCount = table.Column<int>(nullable: false),
                    IsGameOver = table.Column<bool>(nullable: false),
                    StartDate = table.Column<DateTime>(nullable: false),
                    LastPlayDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameplayData", x => x.ID);
                    table.ForeignKey(
                        name: "FK_GameplayData_Field_CurrentPlayerPositionID",
                        column: x => x.CurrentPlayerPositionID,
                        principalTable: "Field",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GameplayData_User_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "IsVisitedField",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IsVisited = table.Column<bool>(nullable: false),
                    ColNumber = table.Column<int>(nullable: false),
                    RowNumber = table.Column<int>(nullable: false),
                    GameplayDataID = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IsVisitedField", x => x.ID);
                    table.ForeignKey(
                        name: "FK_IsVisitedField_GameplayData_GameplayDataID",
                        column: x => x.GameplayDataID,
                        principalTable: "GameplayData",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GameplayData_CurrentPlayerPositionID",
                table: "GameplayData",
                column: "CurrentPlayerPositionID");

            migrationBuilder.CreateIndex(
                name: "IX_GameplayData_PlayerId",
                table: "GameplayData",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_IsVisitedField_GameplayDataID",
                table: "IsVisitedField",
                column: "GameplayDataID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IsVisitedField");

            migrationBuilder.DropTable(
                name: "GameplayData");
        }
    }
}
