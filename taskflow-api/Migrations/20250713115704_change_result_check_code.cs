using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace taskflow_api.Migrations
{
    /// <inheritdoc />
    public partial class change_result_check_code : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CommitCheckResults");

            migrationBuilder.AddColumn<string>(
                name: "ErrorLog",
                table: "CommitRecords",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OutputLog",
                table: "CommitRecords",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ProjectKey",
                table: "CommitRecords",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "Result",
                table: "CommitRecords",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "CommitScanIssues",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CommitRecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Rule = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Severity = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Line = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommitScanIssues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommitScanIssues_CommitRecords_CommitRecordId",
                        column: x => x.CommitRecordId,
                        principalTable: "CommitRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CommitScanIssues_CommitRecordId",
                table: "CommitScanIssues",
                column: "CommitRecordId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CommitScanIssues");

            migrationBuilder.DropColumn(
                name: "ErrorLog",
                table: "CommitRecords");

            migrationBuilder.DropColumn(
                name: "OutputLog",
                table: "CommitRecords");

            migrationBuilder.DropColumn(
                name: "ProjectKey",
                table: "CommitRecords");

            migrationBuilder.DropColumn(
                name: "Result",
                table: "CommitRecords");

            migrationBuilder.CreateTable(
                name: "CommitCheckResults",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CommitRecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ErrorLog = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OutputLog = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Result = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommitCheckResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommitCheckResults_CommitRecords_CommitRecordId",
                        column: x => x.CommitRecordId,
                        principalTable: "CommitRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CommitCheckResults_CommitRecordId",
                table: "CommitCheckResults",
                column: "CommitRecordId");
        }
    }
}
