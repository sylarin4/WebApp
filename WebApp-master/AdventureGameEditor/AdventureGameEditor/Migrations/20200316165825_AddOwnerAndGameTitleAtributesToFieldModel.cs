using Microsoft.EntityFrameworkCore.Migrations;

namespace AdventureGameEditor.Migrations
{
    public partial class AddOwnerAndGameTitleAtributesToFieldModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GameTitle",
                table: "Field",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OwnerId",
                table: "Field",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Field_OwnerId",
                table: "Field",
                column: "OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Field_User_OwnerId",
                table: "Field",
                column: "OwnerId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Field_User_OwnerId",
                table: "Field");

            migrationBuilder.DropIndex(
                name: "IX_Field_OwnerId",
                table: "Field");

            migrationBuilder.DropColumn(
                name: "GameTitle",
                table: "Field");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Field");
        }
    }
}
