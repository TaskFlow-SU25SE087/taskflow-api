using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace taskflow_api.Migrations
{
    /// <inheritdoc />
    public partial class change_File_Task : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "File",
                table: "TaskProjects",
                newName: "AttachmentUrls");

            migrationBuilder.AddColumn<string>(
                name: "AttachmentUrlsList",
                table: "TaskProjects",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AttachmentUrlsList",
                table: "TaskProjects");

            migrationBuilder.RenameColumn(
                name: "AttachmentUrls",
                table: "TaskProjects",
                newName: "File");
        }
    }
}
