using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace taskflow_api.Migrations
{
    /// <inheritdoc />
    public partial class connect_gitHub : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserIntegrations");

            migrationBuilder.DropColumn(
                name: "AccessToken",
                table: "ProjectParts");

            migrationBuilder.AddColumn<Guid>(
                name: "UserGitHubTokenId",
                table: "ProjectParts",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "UserGitHubTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccessToken = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserGitHubTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserGitHubTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectParts_UserGitHubTokenId",
                table: "ProjectParts",
                column: "UserGitHubTokenId");

            migrationBuilder.CreateIndex(
                name: "IX_UserGitHubTokens_UserId",
                table: "UserGitHubTokens",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectParts_UserGitHubTokens_UserGitHubTokenId",
                table: "ProjectParts",
                column: "UserGitHubTokenId",
                principalTable: "UserGitHubTokens",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProjectParts_UserGitHubTokens_UserGitHubTokenId",
                table: "ProjectParts");

            migrationBuilder.DropTable(
                name: "UserGitHubTokens");

            migrationBuilder.DropIndex(
                name: "IX_ProjectParts_UserGitHubTokenId",
                table: "ProjectParts");

            migrationBuilder.DropColumn(
                name: "UserGitHubTokenId",
                table: "ProjectParts");

            migrationBuilder.AddColumn<string>(
                name: "AccessToken",
                table: "ProjectParts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "UserIntegrations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GitHubAccessToken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserIntegrations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserIntegrations_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserIntegrations_UserId",
                table: "UserIntegrations",
                column: "UserId");
        }
    }
}
