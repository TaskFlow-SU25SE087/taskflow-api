using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace taskflow_api.Migrations
{
    /// <inheritdoc />
    public partial class add_board_log : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "BoardId",
                table: "LogProjects",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LogProjects_BoardId",
                table: "LogProjects",
                column: "BoardId");

            migrationBuilder.AddForeignKey(
                name: "FK_LogProjects_Boards_BoardId",
                table: "LogProjects",
                column: "BoardId",
                principalTable: "Boards",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LogProjects_Boards_BoardId",
                table: "LogProjects");

            migrationBuilder.DropIndex(
                name: "IX_LogProjects_BoardId",
                table: "LogProjects");

            migrationBuilder.DropColumn(
                name: "BoardId",
                table: "LogProjects");
        }
    }
}
