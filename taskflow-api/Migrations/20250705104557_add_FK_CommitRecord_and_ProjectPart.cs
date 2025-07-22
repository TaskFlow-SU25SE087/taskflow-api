using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace taskflow_api.Migrations
{
    /// <inheritdoc />
    public partial class add_FK_CommitRecord_and_ProjectPart : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CommitRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectPartId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CommitId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Pusher = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PushedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CommitMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResultSummary = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommitRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommitRecords_ProjectParts_ProjectPartId",
                        column: x => x.ProjectPartId,
                        principalTable: "ProjectParts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CommitRecords_ProjectPartId",
                table: "CommitRecords",
                column: "ProjectPartId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CommitRecords");
        }
    }
}
