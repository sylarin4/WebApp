using Microsoft.EntityFrameworkCore.Migrations;

namespace AdventureGameEditor.Migrations
{
    public partial class ChangeOwnerToUserNameInField : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Field_User_OwnerId",
                table: "Field");

            migrationBuilder.DropIndex(
                name: "IX_Field_OwnerId",
                table: "Field");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Field");

            migrationBuilder.AddColumn<string>(
                name: "UserName",
                table: "Field",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserName",
                table: "Field");

            migrationBuilder.AddColumn<int>(
                name: "OwnerId",
                table: "Field",
                type: "int",
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
    }
}
