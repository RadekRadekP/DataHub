using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RPK_BlazorApp.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreateSqlServer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AllarmData",
                columns: table => new
                {
                    ClientId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ClientDbId = table.Column<int>(type: "int", nullable: false),
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Text = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Operator = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ServerTimestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    allarmDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Nr = table.Column<int>(type: "int", nullable: false),
                    ChangeCounter = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AllarmData", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ClientIdentifiers",
                columns: table => new
                {
                    ClientId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClientName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientIdentifiers", x => x.ClientId);
                });

            migrationBuilder.CreateTable(
                name: "GrindingData",
                columns: table => new
                {
                    ClientId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ClientDbId = table.Column<int>(type: "int", nullable: false),
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProgramName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Operator = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Lotto = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    GwType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ServerTimestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GrindingTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    FinishTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    DateStart = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpperGWStart = table.Column<double>(type: "float", nullable: false),
                    LowerGWStart = table.Column<double>(type: "float", nullable: false),
                    ChangeCounter = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GrindingData", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OperationalData",
                columns: table => new
                {
                    ClientId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ClientDbId = table.Column<int>(type: "int", nullable: false),
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EventId = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Object = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Operator = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ServerTimestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeCounter = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OperationalData", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AllarmData");

            migrationBuilder.DropTable(
                name: "ClientIdentifiers");

            migrationBuilder.DropTable(
                name: "GrindingData");

            migrationBuilder.DropTable(
                name: "OperationalData");
        }
    }
}
