using Microsoft.EntityFrameworkCore.Migrations;

namespace AdventureGameEditor.Migrations
{
    public partial class AddPreludeToGame : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PreludeID",
                table: "Game",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Prelude",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Text = table.Column<string>(nullable: true),
                    GameTitle = table.Column<string>(nullable: true),
                    OwnerId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prelude", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Prelude_User_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Game_PreludeID",
                table: "Game",
                column: "PreludeID");

            migrationBuilder.CreateIndex(
                name: "IX_Prelude_OwnerId",
                table: "Prelude",
                column: "OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Game_Prelude_PreludeID",
                table: "Game",
                column: "PreludeID",
                principalTable: "Prelude",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Game_Prelude_PreludeID",
                table: "Game");

            migrationBuilder.DropTable(
                name: "Prelude");

            migrationBuilder.DropIndex(
                name: "IX_Game_PreludeID",
                table: "Game");

            migrationBuilder.DropColumn(
                name: "PreludeID",
                table: "Game");
        }
    }
}
