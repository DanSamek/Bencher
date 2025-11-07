using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WebApplication.Migrations
{
    /// <inheritdoc />
    public partial class OpeningBookLazyDataLoading : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Data",
                table: "OpeningBooks");

            migrationBuilder.CreateTable(
                name: "OpeningBookContent",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OpeningBookId = table.Column<int>(type: "integer", nullable: false),
                    Data = table.Column<byte[]>(type: "bytea", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpeningBookContent", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OpeningBookContent_OpeningBooks_OpeningBookId",
                        column: x => x.OpeningBookId,
                        principalTable: "OpeningBooks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OpeningBookContent_OpeningBookId",
                table: "OpeningBookContent",
                column: "OpeningBookId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OpeningBookContent");

            migrationBuilder.AddColumn<byte[]>(
                name: "Data",
                table: "OpeningBooks",
                type: "bytea",
                nullable: false,
                defaultValue: new byte[0]);
        }
    }
}
