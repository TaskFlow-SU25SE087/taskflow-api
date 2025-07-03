using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace taskflow_api.Migrations
{
    /// <inheritdoc />
    public partial class change_list_language_of_project_and_framework : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccessToken",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "Branch",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "Framework",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "ProgrammingLanguage",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "RepoProvider",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "RepoUrl",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "WebhookUrl",
                table: "Projects");

            migrationBuilder.CreateTable(
                name: "ProjectParts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProgrammingLanguage = table.Column<int>(type: "int", nullable: false),
                    Framework = table.Column<int>(type: "int", nullable: false),
                    RepoProvider = table.Column<int>(type: "int", nullable: true),
                    RepoUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AccessToken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Branch = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WebhookUrl = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectParts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectParts_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectParts_ProjectId",
                table: "ProjectParts",
                column: "ProjectId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProjectParts");

            migrationBuilder.AddColumn<string>(
                name: "AccessToken",
                table: "Projects",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Branch",
                table: "Projects",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Framework",
                table: "Projects",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProgrammingLanguage",
                table: "Projects",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RepoProvider",
                table: "Projects",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RepoUrl",
                table: "Projects",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WebhookUrl",
                table: "Projects",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
