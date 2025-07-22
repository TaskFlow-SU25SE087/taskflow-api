using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace taskflow_api.Migrations
{
    /// <inheritdoc />
    public partial class fix_term_table_users : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Term",
                table: "AspNetUsers",
                newName: "TermSeason");

            migrationBuilder.AddColumn<int>(
                name: "TermYear",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TermYear",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "TermSeason",
                table: "AspNetUsers",
                newName: "Term");
        }
    }
}
