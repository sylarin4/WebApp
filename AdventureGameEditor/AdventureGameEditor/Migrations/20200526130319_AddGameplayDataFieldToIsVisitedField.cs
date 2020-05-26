using Microsoft.EntityFrameworkCore.Migrations;

namespace AdventureGameEditor.Migrations
{
    public partial class AddGameplayDataFieldToIsVisitedField : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TrialResult_Field_TeleportTargetID",
                table: "TrialResult");

            migrationBuilder.DropIndex(
                name: "IX_TrialResult_TeleportTargetID",
                table: "TrialResult");

            migrationBuilder.DropColumn(
                name: "TeleportTargetID",
                table: "TrialResult");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TeleportTargetID",
                table: "TrialResult",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrialResult_TeleportTargetID",
                table: "TrialResult",
                column: "TeleportTargetID");

            migrationBuilder.AddForeignKey(
                name: "FK_TrialResult_Field_TeleportTargetID",
                table: "TrialResult",
                column: "TeleportTargetID",
                principalTable: "Field",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
