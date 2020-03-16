using Microsoft.EntityFrameworkCore.Migrations;

namespace AdventureGameEditor.Migrations
{
    public partial class RenameHorizontalCordAndVerticalCordToRowNumberAndColNumberInFieldModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HorizontalCord",
                table: "Field");

            migrationBuilder.DropColumn(
                name: "VerticalCord",
                table: "Field");

            migrationBuilder.AddColumn<int>(
                name: "ColNumber",
                table: "Field",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RowNumber",
                table: "Field",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ColNumber",
                table: "Field");

            migrationBuilder.DropColumn(
                name: "RowNumber",
                table: "Field");

            migrationBuilder.AddColumn<int>(
                name: "HorizontalCord",
                table: "Field",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "VerticalCord",
                table: "Field",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
