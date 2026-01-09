using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataHub.Platform.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUserSavedCriteria : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserSavedCriteria",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    TableName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    CriteriaName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    CriteriaJson = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSavedCriteria", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserSavedCriteria");
        }
    }
}
