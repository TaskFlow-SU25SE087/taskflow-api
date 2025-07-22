using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace taskflow_api.Migrations
{
    /// <inheritdoc />
    public partial class delete_idtable_tasktag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_TaskTags",
                table: "TaskTags");

            migrationBuilder.DropIndex(
                name: "IX_TaskTags_TaskId",
                table: "TaskTags");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "TaskTags");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TaskTags",
                table: "TaskTags",
                columns: new[] { "TaskId", "TagId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_TaskTags",
                table: "TaskTags");

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "TaskTags",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_TaskTags",
                table: "TaskTags",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_TaskTags_TaskId",
                table: "TaskTags",
                column: "TaskId");
        }
    }
}
