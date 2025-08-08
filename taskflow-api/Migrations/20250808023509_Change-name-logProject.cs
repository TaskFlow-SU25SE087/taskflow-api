using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace taskflow_api.Migrations
{
    /// <inheritdoc />
    public partial class ChangenamelogProject : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Created_at",
                table: "LogProjects",
                newName: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_LogProjects_ProjectMemberId",
                table: "LogProjects",
                column: "ProjectMemberId");

            migrationBuilder.AddForeignKey(
                name: "FK_LogProjects_ProjectMembers_ProjectMemberId",
                table: "LogProjects",
                column: "ProjectMemberId",
                principalTable: "ProjectMembers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LogProjects_ProjectMembers_ProjectMemberId",
                table: "LogProjects");

            migrationBuilder.DropIndex(
                name: "IX_LogProjects_ProjectMemberId",
                table: "LogProjects");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "LogProjects",
                newName: "Created_at");
        }
    }
}
