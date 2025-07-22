using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace taskflow_api.Migrations
{
    /// <inheritdoc />
    public partial class SonarQube_and_create_table_recordCommit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Notes",
                table: "CommitRecords",
                newName: "CommitUrl");

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpectedFinishAt",
                table: "CommitRecords",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "CommitRecords",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "CommitCheckResults",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CommitRecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ResultType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "UserIntegrations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GitHubAccessToken = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserIntegrations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserIntegrations_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CommitCheckResults_CommitRecordId",
                table: "CommitCheckResults",
                column: "CommitRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_UserIntegrations_UserId",
                table: "UserIntegrations",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CommitCheckResults");

            migrationBuilder.DropTable(
                name: "UserIntegrations");

            migrationBuilder.DropColumn(
                name: "ExpectedFinishAt",
                table: "CommitRecords");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "CommitRecords");

            migrationBuilder.RenameColumn(
                name: "CommitUrl",
                table: "CommitRecords",
                newName: "Notes");
        }
    }
}
