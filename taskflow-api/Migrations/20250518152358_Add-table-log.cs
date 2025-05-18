using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace taskflow_api.Migrations
{
    /// <inheritdoc />
    public partial class Addtablelog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Issue_TaskProjects_TaskProjectID",
                table: "Issue");

            migrationBuilder.DropForeignKey(
                name: "FK_TaskUsers_Projects_ProjectId",
                table: "TaskUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_TaskUsers_TaskProjects_TaskProjectID",
                table: "TaskUsers");

            migrationBuilder.DropTable(
                name: "TaskUser");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Issue",
                table: "Issue");

            migrationBuilder.DropColumn(
                name: "NewBoard",
                table: "TaskUsers");

            migrationBuilder.DropColumn(
                name: "OldBoard",
                table: "TaskUsers");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "TaskUsers");

            migrationBuilder.RenameTable(
                name: "Issue",
                newName: "Issues");

            migrationBuilder.RenameColumn(
                name: "ProjectMemberId",
                table: "TaskUsers",
                newName: "ProjectMemberID");

            migrationBuilder.RenameColumn(
                name: "Timestamp",
                table: "TaskUsers",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "TaskProjectID",
                table: "TaskUsers",
                newName: "IssueID");

            migrationBuilder.RenameColumn(
                name: "ProjectId",
                table: "TaskUsers",
                newName: "TaskId");

            migrationBuilder.RenameIndex(
                name: "IX_TaskUsers_TaskProjectID",
                table: "TaskUsers",
                newName: "IX_TaskUsers_IssueID");

            migrationBuilder.RenameIndex(
                name: "IX_TaskUsers_ProjectId",
                table: "TaskUsers",
                newName: "IX_TaskUsers_TaskId");

            migrationBuilder.RenameIndex(
                name: "IX_Issue_TaskProjectID",
                table: "Issues",
                newName: "IX_Issues_TaskProjectID");

            migrationBuilder.AddColumn<string>(
                name: "Comment",
                table: "TaskUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "TaskUsers",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "TaskUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Issues",
                table: "Issues",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "LogProjects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectMemberId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OldBoard = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    NewBoard = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Assigner = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TaskProjectID = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogProjects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LogProjects_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LogProjects_TaskProjects_TaskProjectID",
                        column: x => x.TaskProjectID,
                        principalTable: "TaskProjects",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_TaskUsers_ProjectMemberID",
                table: "TaskUsers",
                column: "ProjectMemberID");

            migrationBuilder.CreateIndex(
                name: "IX_LogProjects_ProjectId",
                table: "LogProjects",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_LogProjects_TaskProjectID",
                table: "LogProjects",
                column: "TaskProjectID");

            migrationBuilder.AddForeignKey(
                name: "FK_Issues_TaskProjects_TaskProjectID",
                table: "Issues",
                column: "TaskProjectID",
                principalTable: "TaskProjects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TaskUsers_Issues_IssueID",
                table: "TaskUsers",
                column: "IssueID",
                principalTable: "Issues",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TaskUsers_ProjectMembers_ProjectMemberID",
                table: "TaskUsers",
                column: "ProjectMemberID",
                principalTable: "ProjectMembers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TaskUsers_TaskProjects_TaskId",
                table: "TaskUsers",
                column: "TaskId",
                principalTable: "TaskProjects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Issues_TaskProjects_TaskProjectID",
                table: "Issues");

            migrationBuilder.DropForeignKey(
                name: "FK_TaskUsers_Issues_IssueID",
                table: "TaskUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_TaskUsers_ProjectMembers_ProjectMemberID",
                table: "TaskUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_TaskUsers_TaskProjects_TaskId",
                table: "TaskUsers");

            migrationBuilder.DropTable(
                name: "LogProjects");

            migrationBuilder.DropIndex(
                name: "IX_TaskUsers_ProjectMemberID",
                table: "TaskUsers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Issues",
                table: "Issues");

            migrationBuilder.DropColumn(
                name: "Comment",
                table: "TaskUsers");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "TaskUsers");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "TaskUsers");

            migrationBuilder.RenameTable(
                name: "Issues",
                newName: "Issue");

            migrationBuilder.RenameColumn(
                name: "ProjectMemberID",
                table: "TaskUsers",
                newName: "ProjectMemberId");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "TaskUsers",
                newName: "Timestamp");

            migrationBuilder.RenameColumn(
                name: "TaskId",
                table: "TaskUsers",
                newName: "ProjectId");

            migrationBuilder.RenameColumn(
                name: "IssueID",
                table: "TaskUsers",
                newName: "TaskProjectID");

            migrationBuilder.RenameIndex(
                name: "IX_TaskUsers_TaskId",
                table: "TaskUsers",
                newName: "IX_TaskUsers_ProjectId");

            migrationBuilder.RenameIndex(
                name: "IX_TaskUsers_IssueID",
                table: "TaskUsers",
                newName: "IX_TaskUsers_TaskProjectID");

            migrationBuilder.RenameIndex(
                name: "IX_Issues_TaskProjectID",
                table: "Issue",
                newName: "IX_Issue_TaskProjectID");

            migrationBuilder.AddColumn<Guid>(
                name: "NewBoard",
                table: "TaskUsers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OldBoard",
                table: "TaskUsers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "TaskUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Issue",
                table: "Issue",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "TaskUser",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IssueID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ProjectMemberID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TaskId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Assigner = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskUser", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskUser_Issue_IssueID",
                        column: x => x.IssueID,
                        principalTable: "Issue",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TaskUser_ProjectMembers_ProjectMemberID",
                        column: x => x.ProjectMemberID,
                        principalTable: "ProjectMembers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TaskUser_TaskProjects_TaskId",
                        column: x => x.TaskId,
                        principalTable: "TaskProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TaskUser_IssueID",
                table: "TaskUser",
                column: "IssueID");

            migrationBuilder.CreateIndex(
                name: "IX_TaskUser_ProjectMemberID",
                table: "TaskUser",
                column: "ProjectMemberID");

            migrationBuilder.CreateIndex(
                name: "IX_TaskUser_TaskId",
                table: "TaskUser",
                column: "TaskId");

            migrationBuilder.AddForeignKey(
                name: "FK_Issue_TaskProjects_TaskProjectID",
                table: "Issue",
                column: "TaskProjectID",
                principalTable: "TaskProjects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TaskUsers_Projects_ProjectId",
                table: "TaskUsers",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TaskUsers_TaskProjects_TaskProjectID",
                table: "TaskUsers",
                column: "TaskProjectID",
                principalTable: "TaskProjects",
                principalColumn: "Id");
        }
    }
}
