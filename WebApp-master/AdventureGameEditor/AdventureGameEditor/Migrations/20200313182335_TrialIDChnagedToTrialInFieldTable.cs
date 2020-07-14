using Microsoft.EntityFrameworkCore.Migrations;

namespace AdventureGameEditor.Migrations
{
    public partial class TrialIDChnagedToTrialInFieldTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Trial",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TrialType = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trial", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "TrialResult",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ResultType = table.Column<int>(nullable: false),
                    TeleportTargetID = table.Column<int>(nullable: true),
                    Text = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrialResult", x => x.ID);
                    table.ForeignKey(
                        name: "FK_TrialResult_Field_TeleportTargetID",
                        column: x => x.TeleportTargetID,
                        principalTable: "Field",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Alternative",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Text = table.Column<string>(nullable: true),
                    TrialResultID = table.Column<int>(nullable: true),
                    TrialID = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Alternative", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Alternative_Trial_TrialID",
                        column: x => x.TrialID,
                        principalTable: "Trial",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Alternative_TrialResult_TrialResultID",
                        column: x => x.TrialResultID,
                        principalTable: "TrialResult",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Field_TrialID",
                table: "Field",
                column: "TrialID");

            migrationBuilder.CreateIndex(
                name: "IX_Alternative_TrialID",
                table: "Alternative",
                column: "TrialID");

            migrationBuilder.CreateIndex(
                name: "IX_Alternative_TrialResultID",
                table: "Alternative",
                column: "TrialResultID");

            migrationBuilder.CreateIndex(
                name: "IX_TrialResult_TeleportTargetID",
                table: "TrialResult",
                column: "TeleportTargetID");

            migrationBuilder.AddForeignKey(
                name: "FK_Field_Trial_TrialID",
                table: "Field",
                column: "TrialID",
                principalTable: "Trial",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Field_Trial_TrialID",
                table: "Field");

            migrationBuilder.DropTable(
                name: "Alternative");

            migrationBuilder.DropTable(
                name: "Trial");

            migrationBuilder.DropTable(
                name: "TrialResult");

            migrationBuilder.DropIndex(
                name: "IX_Field_TrialID",
                table: "Field");
        }
    }
}
