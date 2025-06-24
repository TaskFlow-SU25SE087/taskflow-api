using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace taskflow_api.Migrations
{
    /// <inheritdoc />
    public partial class list_File_comment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Commenter",
                table: "TaskComments",
                newName: "CommenterId");

            migrationBuilder.AddColumn<string>(
                name: "AttachmentUrls",
                table: "TaskComments",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AttachmentUrls",
                table: "TaskComments");

            migrationBuilder.RenameColumn(
                name: "CommenterId",
                table: "TaskComments",
                newName: "Commenter");
        }
    }
}
