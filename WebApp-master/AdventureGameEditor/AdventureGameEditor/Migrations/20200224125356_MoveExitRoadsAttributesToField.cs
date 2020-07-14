using Microsoft.EntityFrameworkCore.Migrations;

namespace AdventureGameEditor.Migrations
{
    public partial class MoveExitRoadsAttributesToField : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Field_ExitRoads_ExitRoadsID",
                table: "Field");

            migrationBuilder.DropTable(
                name: "ExitRoads");

            migrationBuilder.DropIndex(
                name: "IX_Field_ExitRoadsID",
                table: "Field");

            migrationBuilder.DropColumn(
                name: "ExitRoadsID",
                table: "Field");

            migrationBuilder.AddColumn<bool>(
                name: "IsDownWay",
                table: "Field",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsLeftWay",
                table: "Field",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsRightWay",
                table: "Field",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsUpWay",
                table: "Field",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDownWay",
                table: "Field");

            migrationBuilder.DropColumn(
                name: "IsLeftWay",
                table: "Field");

            migrationBuilder.DropColumn(
                name: "IsRightWay",
                table: "Field");

            migrationBuilder.DropColumn(
                name: "IsUpWay",
                table: "Field");

            migrationBuilder.AddColumn<int>(
                name: "ExitRoadsID",
                table: "Field",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ExitRoads",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IsDownWay = table.Column<bool>(type: "bit", nullable: false),
                    IsLeftWay = table.Column<bool>(type: "bit", nullable: false),
                    IsRightWay = table.Column<bool>(type: "bit", nullable: false),
                    IsUpWay = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExitRoads", x => x.ID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Field_ExitRoadsID",
                table: "Field",
                column: "ExitRoadsID");

            migrationBuilder.AddForeignKey(
                name: "FK_Field_ExitRoads_ExitRoadsID",
                table: "Field",
                column: "ExitRoadsID",
                principalTable: "ExitRoads",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
