using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace taskflow_api.Migrations
{
    /// <inheritdoc />
    public partial class projectLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Assigner",
                table: "LogProjects");

            migrationBuilder.DropColumn(
                name: "NewBoard",
                table: "LogProjects");

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "LogProjects",
                newName: "ActionType");

            migrationBuilder.RenameColumn(
                name: "Timestamp",
                table: "LogProjects",
                newName: "Created_at");

            migrationBuilder.RenameColumn(
                name: "OldBoard",
                table: "LogProjects",
                newName: "SprintId");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "LogProjects",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FieldChanged",
                table: "LogProjects",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NewValue",
                table: "LogProjects",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OldValue",
                table: "LogProjects",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ProcessingFiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UrlFile = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    statusFile = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessingFiles", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LogProjects_SprintId",
                table: "LogProjects",
                column: "SprintId");

            migrationBuilder.AddForeignKey(
                name: "FK_LogProjects_Sprints_SprintId",
                table: "LogProjects",
                column: "SprintId",
                principalTable: "Sprints",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LogProjects_Sprints_SprintId",
                table: "LogProjects");

            migrationBuilder.DropTable(
                name: "ProcessingFiles");

            migrationBuilder.DropIndex(
                name: "IX_LogProjects_SprintId",
                table: "LogProjects");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "LogProjects");

            migrationBuilder.DropColumn(
                name: "FieldChanged",
                table: "LogProjects");

            migrationBuilder.DropColumn(
                name: "NewValue",
                table: "LogProjects");

            migrationBuilder.DropColumn(
                name: "OldValue",
                table: "LogProjects");

            migrationBuilder.RenameColumn(
                name: "SprintId",
                table: "LogProjects",
                newName: "OldBoard");

            migrationBuilder.RenameColumn(
                name: "Created_at",
                table: "LogProjects",
                newName: "Timestamp");

            migrationBuilder.RenameColumn(
                name: "ActionType",
                table: "LogProjects",
                newName: "Type");

            migrationBuilder.AddColumn<Guid>(
                name: "Assigner",
                table: "LogProjects",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "NewBoard",
                table: "LogProjects",
                type: "uniqueidentifier",
                nullable: true);
        }
    }
}
