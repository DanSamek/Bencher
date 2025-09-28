using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication.Migrations
{
    /// <inheritdoc />
    public partial class EngineBuildScriptAsFile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AutobenchState_Tests_TestId",
                table: "AutobenchState");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AutobenchState",
                table: "AutobenchState");

            migrationBuilder.DropColumn(
                name: "Resolved",
                table: "AutobenchState");

            migrationBuilder.RenameTable(
                name: "AutobenchState",
                newName: "AutobenchStates");

            migrationBuilder.RenameIndex(
                name: "IX_AutobenchState_TestId",
                table: "AutobenchStates",
                newName: "IX_AutobenchStates_TestId");

            migrationBuilder.AlterColumn<byte[]>(
                name: "BuildScript",
                table: "Engines",
                type: "BLOB",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 1024);

            migrationBuilder.AddPrimaryKey(
                name: "PK_AutobenchStates",
                table: "AutobenchStates",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AutobenchStates_Tests_TestId",
                table: "AutobenchStates",
                column: "TestId",
                principalTable: "Tests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AutobenchStates_Tests_TestId",
                table: "AutobenchStates");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AutobenchStates",
                table: "AutobenchStates");

            migrationBuilder.RenameTable(
                name: "AutobenchStates",
                newName: "AutobenchState");

            migrationBuilder.RenameIndex(
                name: "IX_AutobenchStates_TestId",
                table: "AutobenchState",
                newName: "IX_AutobenchState_TestId");

            migrationBuilder.AlterColumn<string>(
                name: "BuildScript",
                table: "Engines",
                type: "TEXT",
                maxLength: 1024,
                nullable: false,
                oldClrType: typeof(byte[]),
                oldType: "BLOB");

            migrationBuilder.AddColumn<bool>(
                name: "Resolved",
                table: "AutobenchState",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_AutobenchState",
                table: "AutobenchState",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AutobenchState_Tests_TestId",
                table: "AutobenchState",
                column: "TestId",
                principalTable: "Tests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
