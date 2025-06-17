using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace taskflow_api.Migrations
{
    /// <inheritdoc />
    public partial class RenameImplementerandAssignertoId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaskAssignees_ProjectMembers_Implementer",
                table: "TaskAssignees");

            migrationBuilder.DropIndex(
                name: "IX_TaskAssignees_Implementer",
                table: "TaskAssignees");

            migrationBuilder.RenameColumn(
                name: "Assigner",
                table: "TaskAssignees",
                newName: "AssignerId");

            migrationBuilder.AddColumn<Guid>(
                name: "ImplementerId",
                table: "TaskAssignees",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_TaskAssignees_ImplementerId",
                table: "TaskAssignees",
                column: "ImplementerId");

            migrationBuilder.AddForeignKey(
                name: "FK_TaskAssignees_ProjectMembers_ImplementerId",
                table: "TaskAssignees",
                column: "ImplementerId",
                principalTable: "ProjectMembers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaskAssignees_ProjectMembers_ImplementerId",
                table: "TaskAssignees");

            migrationBuilder.DropIndex(
                name: "IX_TaskAssignees_ImplementerId",
                table: "TaskAssignees");

            migrationBuilder.DropColumn(
                name: "ImplementerId",
                table: "TaskAssignees");

            migrationBuilder.RenameColumn(
                name: "AssignerId",
                table: "TaskAssignees",
                newName: "Assigner");

            migrationBuilder.CreateIndex(
                name: "IX_TaskAssignees_Implementer",
                table: "TaskAssignees",
                column: "Implementer");

            migrationBuilder.AddForeignKey(
                name: "FK_TaskAssignees_ProjectMembers_Implementer",
                table: "TaskAssignees",
                column: "Implementer",
                principalTable: "ProjectMembers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
