using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace taskflow_api.Migrations
{
    /// <inheritdoc />
    public partial class addfieldresultCommit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Bugs",
                table: "CommitRecords",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CodeSmells",
                table: "CommitRecords",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "Coverage",
                table: "CommitRecords",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "DuplicatedBlocks",
                table: "CommitRecords",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DuplicatedLines",
                table: "CommitRecords",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "DuplicatedLinesDensity",
                table: "CommitRecords",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "QualityGateStatus",
                table: "CommitRecords",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<TimeSpan>(
                name: "ScanDuration",
                table: "CommitRecords",
                type: "time",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<int>(
                name: "SecurityHotspots",
                table: "CommitRecords",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Vulnerabilities",
                table: "CommitRecords",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Bugs",
                table: "CommitRecords");

            migrationBuilder.DropColumn(
                name: "CodeSmells",
                table: "CommitRecords");

            migrationBuilder.DropColumn(
                name: "Coverage",
                table: "CommitRecords");

            migrationBuilder.DropColumn(
                name: "DuplicatedBlocks",
                table: "CommitRecords");

            migrationBuilder.DropColumn(
                name: "DuplicatedLines",
                table: "CommitRecords");

            migrationBuilder.DropColumn(
                name: "DuplicatedLinesDensity",
                table: "CommitRecords");

            migrationBuilder.DropColumn(
                name: "QualityGateStatus",
                table: "CommitRecords");

            migrationBuilder.DropColumn(
                name: "ScanDuration",
                table: "CommitRecords");

            migrationBuilder.DropColumn(
                name: "SecurityHotspots",
                table: "CommitRecords");

            migrationBuilder.DropColumn(
                name: "Vulnerabilities",
                table: "CommitRecords");
        }
    }
}
