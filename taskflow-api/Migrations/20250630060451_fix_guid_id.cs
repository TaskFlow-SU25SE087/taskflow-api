using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace taskflow_api.Migrations
{
    /// <inheritdoc />
    public partial class fix_guid_id : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TermSeason",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "TermYear",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<Guid>(
                name: "TermId",
                table: "Projects",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TermId",
                table: "AspNetUsers",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "Terms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Season = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Terms", x => x.Id);
                });
            migrationBuilder.InsertData(
                table: "Terms",
                columns: new[] { "Id", "Season", "Year", "StartDate", "EndDate", "IsActive" },
                values: new object[]
                {
                    new Guid("00000000-0000-0000-0000-000000000000"),
                    "Default",
                    2000,
                    new DateTime(2000, 1, 1),
                    new DateTime(2000, 1, 2),
                    false
                });


            migrationBuilder.CreateIndex(
                name: "IX_Projects_TermId",
                table: "Projects",
                column: "TermId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_TermId",
                table: "AspNetUsers",
                column: "TermId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Terms_TermId",
                table: "AspNetUsers",
                column: "TermId",
                principalTable: "Terms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_Terms_TermId",
                table: "Projects",
                column: "TermId",
                principalTable: "Terms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Terms_TermId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_Projects_Terms_TermId",
                table: "Projects");

            migrationBuilder.DropTable(
                name: "Terms");

            migrationBuilder.DropIndex(
                name: "IX_Projects_TermId",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_TermId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "TermId",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "TermId",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<string>(
                name: "TermSeason",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TermYear",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
