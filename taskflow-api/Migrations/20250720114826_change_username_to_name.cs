using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace taskflow_api.Migrations
{
    /// <inheritdoc />
    public partial class change_username_to_name : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UserNameLocal",
                table: "GitMembers",
                newName: "NameLocal");

            migrationBuilder.RenameColumn(
                name: "GitUserName",
                table: "GitMembers",
                newName: "GitName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "NameLocal",
                table: "GitMembers",
                newName: "UserNameLocal");

            migrationBuilder.RenameColumn(
                name: "GitName",
                table: "GitMembers",
                newName: "GitUserName");
        }
    }
}
