using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace taskflow_api.Migrations
{
    /// <inheritdoc />
    public partial class fixtableTaskAssignee : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaskAssignees_Issues_IssueID",
                table: "TaskAssignees");

            migrationBuilder.DropForeignKey(
                name: "FK_TaskAssignees_TaskProjects_TaskId",
                table: "TaskAssignees");

            migrationBuilder.DropIndex(
                name: "IX_TaskAssignees_TaskId",
                table: "TaskAssignees");

            migrationBuilder.RenameColumn(
                name: "IssueID",
                table: "TaskAssignees",
                newName: "IssueId");

            migrationBuilder.RenameColumn(
                name: "TaskId",
                table: "TaskAssignees",
                newName: "RefId");

            migrationBuilder.RenameIndex(
                name: "IX_TaskAssignees_IssueID",
                table: "TaskAssignees",
                newName: "IX_TaskAssignees_IssueId");

            migrationBuilder.AddColumn<Guid>(
                name: "TaskProjectId",
                table: "TaskAssignees",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "TaskAssignees",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_TaskAssignees_TaskProjectId",
                table: "TaskAssignees",
                column: "TaskProjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_TaskAssignees_Issues_IssueId",
                table: "TaskAssignees",
                column: "IssueId",
                principalTable: "Issues",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TaskAssignees_TaskProjects_TaskProjectId",
                table: "TaskAssignees",
                column: "TaskProjectId",
                principalTable: "TaskProjects",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaskAssignees_Issues_IssueId",
                table: "TaskAssignees");

            migrationBuilder.DropForeignKey(
                name: "FK_TaskAssignees_TaskProjects_TaskProjectId",
                table: "TaskAssignees");

            migrationBuilder.DropIndex(
                name: "IX_TaskAssignees_TaskProjectId",
                table: "TaskAssignees");

            migrationBuilder.DropColumn(
                name: "TaskProjectId",
                table: "TaskAssignees");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "TaskAssignees");

            migrationBuilder.RenameColumn(
                name: "IssueId",
                table: "TaskAssignees",
                newName: "IssueID");

            migrationBuilder.RenameColumn(
                name: "RefId",
                table: "TaskAssignees",
                newName: "TaskId");

            migrationBuilder.RenameIndex(
                name: "IX_TaskAssignees_IssueId",
                table: "TaskAssignees",
                newName: "IX_TaskAssignees_IssueID");

            migrationBuilder.CreateIndex(
                name: "IX_TaskAssignees_TaskId",
                table: "TaskAssignees",
                column: "TaskId");

            migrationBuilder.AddForeignKey(
                name: "FK_TaskAssignees_Issues_IssueID",
                table: "TaskAssignees",
                column: "IssueID",
                principalTable: "Issues",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TaskAssignees_TaskProjects_TaskId",
                table: "TaskAssignees",
                column: "TaskId",
                principalTable: "TaskProjects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
