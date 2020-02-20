using Microsoft.EntityFrameworkCore.Migrations;

namespace AdventureGameEditor.Migrations
{
    public partial class ReplaceFieldCoordinateInFieldModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Field_FieldCoordinate_FieldCoordinateID",
                table: "Field");

            migrationBuilder.DropTable(
                name: "FieldCoordinate");

            migrationBuilder.DropIndex(
                name: "IX_Field_FieldCoordinateID",
                table: "Field");

            migrationBuilder.DropColumn(
                name: "FieldCoordinateID",
                table: "Field");

            migrationBuilder.AddColumn<int>(
                name: "HorizontalCord",
                table: "Field",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "VerticalCord",
                table: "Field",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HorizontalCord",
                table: "Field");

            migrationBuilder.DropColumn(
                name: "VerticalCord",
                table: "Field");

            migrationBuilder.AddColumn<int>(
                name: "FieldCoordinateID",
                table: "Field",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "FieldCoordinate",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HorizontalCoordinate = table.Column<int>(type: "int", nullable: false),
                    VerticalCoordinate = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FieldCoordinate", x => x.ID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Field_FieldCoordinateID",
                table: "Field",
                column: "FieldCoordinateID");

            migrationBuilder.AddForeignKey(
                name: "FK_Field_FieldCoordinate_FieldCoordinateID",
                table: "Field",
                column: "FieldCoordinateID",
                principalTable: "FieldCoordinate",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
