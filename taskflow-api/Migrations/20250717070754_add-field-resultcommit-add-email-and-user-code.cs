using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace taskflow_api.Migrations
{
    /// <inheritdoc />
    public partial class addfieldresultcommitaddemailandusercode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BlamedGitEmail",
                table: "CommitScanIssues",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BlamedGitName",
                table: "CommitScanIssues",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BlamedGitEmail",
                table: "CommitScanIssues");

            migrationBuilder.DropColumn(
                name: "BlamedGitName",
                table: "CommitScanIssues");
        }
    }
}
