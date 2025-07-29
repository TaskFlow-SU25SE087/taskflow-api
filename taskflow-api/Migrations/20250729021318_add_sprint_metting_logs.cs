using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace taskflow_api.Migrations
{
    /// <inheritdoc />
    public partial class add_sprint_metting_logs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Issues_TaskProjects_TaskProjectID",
                table: "Issues");

            migrationBuilder.RenameColumn(
                name: "TaskProjectID",
                table: "Issues",
                newName: "TaskProjectId");

            migrationBuilder.RenameIndex(
                name: "IX_Issues_TaskProjectID",
                table: "Issues",
                newName: "IX_Issues_TaskProjectId");

            migrationBuilder.CreateTable(
                name: "SprintMeetingLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SprintId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompletedTasksJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UnfinishedTasksJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NextPlan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SprintMeetingLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SprintMeetingLogs_Sprints_SprintId",
                        column: x => x.SprintId,
                        principalTable: "Sprints",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Issues_CreatedBy",
                table: "Issues",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_SprintMeetingLogs_SprintId",
                table: "SprintMeetingLogs",
                column: "SprintId");

            migrationBuilder.AddForeignKey(
                name: "FK_Issues_ProjectMembers_CreatedBy",
                table: "Issues",
                column: "CreatedBy",
                principalTable: "ProjectMembers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Issues_TaskProjects_TaskProjectId",
                table: "Issues",
                column: "TaskProjectId",
                principalTable: "TaskProjects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Issues_ProjectMembers_CreatedBy",
                table: "Issues");

            migrationBuilder.DropForeignKey(
                name: "FK_Issues_TaskProjects_TaskProjectId",
                table: "Issues");

            migrationBuilder.DropTable(
                name: "SprintMeetingLogs");

            migrationBuilder.DropIndex(
                name: "IX_Issues_CreatedBy",
                table: "Issues");

            migrationBuilder.RenameColumn(
                name: "TaskProjectId",
                table: "Issues",
                newName: "TaskProjectID");

            migrationBuilder.RenameIndex(
                name: "IX_Issues_TaskProjectId",
                table: "Issues",
                newName: "IX_Issues_TaskProjectID");

            migrationBuilder.AddForeignKey(
                name: "FK_Issues_TaskProjects_TaskProjectID",
                table: "Issues",
                column: "TaskProjectID",
                principalTable: "TaskProjects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
