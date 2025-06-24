using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace taskflow_api.Migrations
{
    /// <inheritdoc />
    public partial class RenameUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AttachmentUrlsList",
                table: "TaskProjects");

            migrationBuilder.RenameColumn(
                name: "AttachmentUrls",
                table: "TaskProjects",
                newName: "CompletionAttachmentUrls");

            migrationBuilder.AddColumn<string>(
                name: "AttachmentUrl",
                table: "TaskProjects",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AttachmentUrl",
                table: "TaskProjects");

            migrationBuilder.RenameColumn(
                name: "CompletionAttachmentUrls",
                table: "TaskProjects",
                newName: "AttachmentUrls");

            migrationBuilder.AddColumn<string>(
                name: "AttachmentUrlsList",
                table: "TaskProjects",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
