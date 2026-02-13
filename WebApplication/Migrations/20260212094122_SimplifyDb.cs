using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication.Migrations
{
    /// <inheritdoc />
    public partial class SimplifyDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_WorkerErrors_WorkerLogId",
                table: "WorkerErrors");

            migrationBuilder.DropColumn(
                name: "InitialTestState",
                table: "WorkerLogs");

            migrationBuilder.CreateIndex(
                name: "IX_WorkerErrors_WorkerLogId",
                table: "WorkerErrors",
                column: "WorkerLogId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_WorkerErrors_WorkerLogId",
                table: "WorkerErrors");

            migrationBuilder.AddColumn<int>(
                name: "InitialTestState",
                table: "WorkerLogs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_WorkerErrors_WorkerLogId",
                table: "WorkerErrors",
                column: "WorkerLogId");
        }
    }
}
