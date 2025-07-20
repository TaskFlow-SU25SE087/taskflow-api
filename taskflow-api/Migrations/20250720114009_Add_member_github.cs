using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace taskflow_api.Migrations
{
    /// <inheritdoc />
    public partial class Add_member_github : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GitMembers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectPartId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectMemberId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    GitUserName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GitEmail = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GitAvatarUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserNameLocal = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EmailLocal = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GitMembers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GitMembers_ProjectMembers_ProjectMemberId",
                        column: x => x.ProjectMemberId,
                        principalTable: "ProjectMembers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GitMembers_ProjectParts_ProjectPartId",
                        column: x => x.ProjectPartId,
                        principalTable: "ProjectParts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GitMembers_ProjectMemberId",
                table: "GitMembers",
                column: "ProjectMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_GitMembers_ProjectPartId",
                table: "GitMembers",
                column: "ProjectPartId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GitMembers");
        }
    }
}
