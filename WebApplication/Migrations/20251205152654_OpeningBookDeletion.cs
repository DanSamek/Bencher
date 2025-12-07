using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication.Migrations
{
    /// <inheritdoc />
    public partial class OpeningBookDeletion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OpeningBooks_AspNetUsers_UserId",
                table: "OpeningBooks");

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

            migrationBuilder.AddForeignKey(
                name: "FK_OpeningBooks_AspNetUsers_UserId",
                table: "OpeningBooks",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
