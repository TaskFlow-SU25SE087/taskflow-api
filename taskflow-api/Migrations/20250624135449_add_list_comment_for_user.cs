using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace taskflow_api.Migrations
{
    /// <inheritdoc />
    public partial class add_list_comment_for_user : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaskComments_ProjectMembers_UserCommentId",
                table: "TaskComments");

            migrationBuilder.DropIndex(
                name: "IX_TaskComments_UserCommentId",
                table: "TaskComments");

            migrationBuilder.DropColumn(
                name: "UserCommentId",
                table: "TaskComments");

            migrationBuilder.CreateIndex(
                name: "IX_TaskComments_CommenterId",
                table: "TaskComments",
                column: "CommenterId");

            migrationBuilder.AddForeignKey(
                name: "FK_TaskComments_ProjectMembers_CommenterId",
                table: "TaskComments",
                column: "CommenterId",
                principalTable: "ProjectMembers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaskComments_ProjectMembers_CommenterId",
                table: "TaskComments");

            migrationBuilder.DropIndex(
                name: "IX_TaskComments_CommenterId",
                table: "TaskComments");

            migrationBuilder.AddColumn<Guid>(
                name: "UserCommentId",
                table: "TaskComments",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_TaskComments_UserCommentId",
                table: "TaskComments",
                column: "UserCommentId");

            migrationBuilder.AddForeignKey(
                name: "FK_TaskComments_ProjectMembers_UserCommentId",
                table: "TaskComments",
                column: "UserCommentId",
                principalTable: "ProjectMembers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
