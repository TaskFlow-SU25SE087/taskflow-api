using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace taskflow_api.Migrations
{
    /// <inheritdoc />
    public partial class AddEffortPointsToTasks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EffortPoints",
                table: "TaskProjects",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AssignedEffortPoints",
                table: "TaskAssignees",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EffortPoints",
                table: "TaskProjects");

            migrationBuilder.DropColumn(
                name: "AssignedEffortPoints",
                table: "TaskAssignees");
        }
    }
}
