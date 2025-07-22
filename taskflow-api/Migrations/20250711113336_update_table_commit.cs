using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace taskflow_api.Migrations
{
    /// <inheritdoc />
    public partial class update_table_commit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Notes",
                table: "CommitCheckResults");

            migrationBuilder.DropColumn(
                name: "Value",
                table: "CommitCheckResults");

            migrationBuilder.RenameColumn(
                name: "ResultType",
                table: "CommitCheckResults",
                newName: "OutputLog");

            migrationBuilder.AddColumn<string>(
                name: "ErrorLog",
                table: "CommitCheckResults",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "Result",
                table: "CommitCheckResults",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ErrorLog",
                table: "CommitCheckResults");

            migrationBuilder.DropColumn(
                name: "Result",
                table: "CommitCheckResults");

            migrationBuilder.RenameColumn(
                name: "OutputLog",
                table: "CommitCheckResults",
                newName: "ResultType");

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "CommitCheckResults",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Value",
                table: "CommitCheckResults",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
