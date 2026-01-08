using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RPK_BlazorApp.Migrations
{
    /// <inheritdoc />
    public partial class AddRawQueryToUserSavedCriteria : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RawQuery",
                table: "UserSavedCriteria",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RawQuery",
                table: "UserSavedCriteria");
        }
    }
}
