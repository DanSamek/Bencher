using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication.Migrations
{
    /// <inheritdoc />
    public partial class AutobenchState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BuildScriptLinux",
                table: "Engines");

            migrationBuilder.RenameColumn(
                name: "BuildScriptWindows",
                table: "Engines",
                newName: "BuildScript");

            migrationBuilder.AddColumn<int>(
                name: "NumberOfThreads",
                table: "WorkerLogs",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalNumberOfGames",
                table: "WorkerLogs",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ThreadScale",
                table: "Tests",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Wl",
                table: "Pentas",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "OpeningBooks",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "AutobenchState",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TestId = table.Column<int>(type: "INTEGER", nullable: false),
                    Bench = table.Column<int>(type: "INTEGER", nullable: false),
                    Confidence = table.Column<double>(type: "REAL", nullable: false),
                    Resolved = table.Column<bool>(type: "INTEGER", nullable: false),
                    UserConfidence = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AutobenchState", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AutobenchState_Tests_TestId",
                        column: x => x.TestId,
                        principalTable: "Tests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OpeningBooks_UserId",
                table: "OpeningBooks",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AutobenchState_TestId",
                table: "AutobenchState",
                column: "TestId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_OpeningBooks_AspNetUsers_UserId",
                table: "OpeningBooks",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OpeningBooks_AspNetUsers_UserId",
                table: "OpeningBooks");

            migrationBuilder.DropTable(
                name: "AutobenchState");

            migrationBuilder.DropIndex(
                name: "IX_OpeningBooks_UserId",
                table: "OpeningBooks");

            migrationBuilder.DropColumn(
                name: "NumberOfThreads",
                table: "WorkerLogs");

            migrationBuilder.DropColumn(
                name: "TotalNumberOfGames",
                table: "WorkerLogs");

            migrationBuilder.DropColumn(
                name: "ThreadScale",
                table: "Tests");

            migrationBuilder.DropColumn(
                name: "Wl",
                table: "Pentas");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "OpeningBooks");

            migrationBuilder.RenameColumn(
                name: "BuildScript",
                table: "Engines",
                newName: "BuildScriptWindows");

            migrationBuilder.AddColumn<string>(
                name: "BuildScriptLinux",
                table: "Engines",
                type: "TEXT",
                maxLength: 1024,
                nullable: false,
                defaultValue: "");
        }
    }
}
