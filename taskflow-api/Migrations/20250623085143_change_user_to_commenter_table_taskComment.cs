using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace taskflow_api.Migrations
{
    /// <inheritdoc />
    public partial class change_user_to_commenter_table_taskComment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaskComments_ProjectMembers_UserId",
                table: "TaskComments");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "TaskComments",
                newName: "UserCommentId");

            migrationBuilder.RenameIndex(
                name: "IX_TaskComments_UserId",
                table: "TaskComments",
                newName: "IX_TaskComments_UserCommentId");

            migrationBuilder.AddForeignKey(
                name: "FK_TaskComments_ProjectMembers_UserCommentId",
                table: "TaskComments",
                column: "UserCommentId",
                principalTable: "ProjectMembers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaskComments_ProjectMembers_UserCommentId",
                table: "TaskComments");

            migrationBuilder.RenameColumn(
                name: "UserCommentId",
                table: "TaskComments",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_TaskComments_UserCommentId",
                table: "TaskComments",
                newName: "IX_TaskComments_UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_TaskComments_ProjectMembers_UserId",
                table: "TaskComments",
                column: "UserId",
                principalTable: "ProjectMembers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
