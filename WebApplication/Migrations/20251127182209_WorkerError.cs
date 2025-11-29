using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication.Migrations
{
    /// <inheritdoc />
    public partial class WorkerError : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ErrorContent_Errors_ErrorId",
                table: "ErrorContent");

            migrationBuilder.DropForeignKey(
                name: "FK_Errors_Tests_TestId",
                table: "Errors");

            migrationBuilder.DropForeignKey(
                name: "FK_Errors_WorkerLogs_WorkerLogId",
                table: "Errors");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Errors",
                table: "Errors");

            migrationBuilder.RenameTable(
                name: "Errors",
                newName: "WorkerErrors");

            migrationBuilder.RenameIndex(
                name: "IX_Errors_WorkerLogId",
                table: "WorkerErrors",
                newName: "IX_WorkerErrors_WorkerLogId");

            migrationBuilder.RenameIndex(
                name: "IX_Errors_TestId",
                table: "WorkerErrors",
                newName: "IX_WorkerErrors_TestId");

            migrationBuilder.AlterColumn<int>(
                name: "WorkerLogId",
                table: "WorkerErrors",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "TestId",
                table: "WorkerErrors",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "WorkerErrors",
                type: "character varying(13)",
                maxLength: 13,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_WorkerErrors",
                table: "WorkerErrors",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ErrorContent_WorkerErrors_ErrorId",
                table: "ErrorContent",
                column: "ErrorId",
                principalTable: "WorkerErrors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkerErrors_Tests_TestId",
                table: "WorkerErrors",
                column: "TestId",
                principalTable: "Tests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkerErrors_WorkerLogs_WorkerLogId",
                table: "WorkerErrors",
                column: "WorkerLogId",
                principalTable: "WorkerLogs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ErrorContent_WorkerErrors_ErrorId",
                table: "ErrorContent");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkerErrors_Tests_TestId",
                table: "WorkerErrors");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkerErrors_WorkerLogs_WorkerLogId",
                table: "WorkerErrors");

            migrationBuilder.DropPrimaryKey(
                name: "PK_WorkerErrors",
                table: "WorkerErrors");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "WorkerErrors");

            migrationBuilder.RenameTable(
                name: "WorkerErrors",
                newName: "Errors");

            migrationBuilder.RenameIndex(
                name: "IX_WorkerErrors_WorkerLogId",
                table: "Errors",
                newName: "IX_Errors_WorkerLogId");

            migrationBuilder.RenameIndex(
                name: "IX_WorkerErrors_TestId",
                table: "Errors",
                newName: "IX_Errors_TestId");

            migrationBuilder.AlterColumn<int>(
                name: "WorkerLogId",
                table: "Errors",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "TestId",
                table: "Errors",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Errors",
                table: "Errors",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ErrorContent_Errors_ErrorId",
                table: "ErrorContent",
                column: "ErrorId",
                principalTable: "Errors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Errors_Tests_TestId",
                table: "Errors",
                column: "TestId",
                principalTable: "Tests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Errors_WorkerLogs_WorkerLogId",
                table: "Errors",
                column: "WorkerLogId",
                principalTable: "WorkerLogs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
