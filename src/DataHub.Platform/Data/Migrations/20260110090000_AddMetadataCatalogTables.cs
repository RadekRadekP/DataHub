using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataHub.Platform.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMetadataCatalogTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create MetaEntities table
            migrationBuilder.CreateTable(
                name: "MetaEntities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntityName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TableName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SchemaName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, defaultValue: "dbo"),
                    DbContextName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ClrTypeName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DisplayName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsView = table.Column<bool>(type: "bit", nullable: false),
                    IsDiscovered = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MetaEntities", x => x.Id);
                });

            // Create MetaFields table
            migrationBuilder.CreateTable(
                name: "MetaFields",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MetaEntityId = table.Column<int>(type: "int", nullable: false),
                    FieldName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ColumnName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SqlType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ClrType = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IsPrimaryKey = table.Column<bool>(type: "bit", nullable: false),
                    IsForeignKey = table.Column<bool>(type: "bit", nullable: false),
                    IsNullable = table.Column<bool>(type: "bit", nullable: false),
                    IsComputed = table.Column<bool>(type: "bit", nullable: false),
                    MaxLength = table.Column<int>(type: "int", nullable: true),
                    Precision = table.Column<int>(type: "int", nullable: true),
                    Scale = table.Column<int>(type: "int", nullable: true),
                    DefaultValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsVisibleInGrid = table.Column<bool>(type: "bit", nullable: false),
                    IsEditable = table.Column<bool>(type: "bit", nullable: false),
                    UiHint = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MetaFields", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MetaFields_MetaEntities_MetaEntityId",
                        column: x => x.MetaEntityId,
                        principalTable: "MetaEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Create MetaRelations table
            migrationBuilder.CreateTable(
                name: "MetaRelations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FromEntityId = table.Column<int>(type: "int", nullable: false),
                    FromFieldId = table.Column<int>(type: "int", nullable: false),
                    ToEntityId = table.Column<int>(type: "int", nullable: false),
                    ToFieldId = table.Column<int>(type: "int", nullable: false),
                    DisplayFieldId = table.Column<int>(type: "int", nullable: true),
                    RelationType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "ManyToOne"),
                    DisplayName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DeleteBehavior = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MetaRelations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MetaRelations_MetaEntities_FromEntityId",
                        column: x => x.FromEntityId,
                        principalTable: "MetaEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MetaRelations_MetaEntities_ToEntityId",
                        column: x => x.ToEntityId,
                        principalTable: "MetaEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MetaRelations_MetaFields_FromFieldId",
                        column: x => x.FromFieldId,
                        principalTable: "MetaFields",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MetaRelations_MetaFields_ToFieldId",
                        column: x => x.ToFieldId,
                        principalTable: "MetaFields",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MetaRelations_MetaFields_DisplayFieldId",
                        column: x => x.DisplayFieldId,
                        principalTable: "MetaFields",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // Create indexes
            migrationBuilder.CreateIndex(
                name: "IX_MetaFields_MetaEntityId",
                table: "MetaFields",
                column: "MetaEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_MetaRelations_FromEntityId",
                table: "MetaRelations",
                column: "FromEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_MetaRelations_ToEntityId",
                table: "MetaRelations",
                column: "ToEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_MetaRelations_FromFieldId",
                table: "MetaRelations",
                column: "FromFieldId");

            migrationBuilder.CreateIndex(
                name: "IX_MetaRelations_ToFieldId",
                table: "MetaRelations",
                column: "ToFieldId");

            migrationBuilder.CreateIndex(
                name: "IX_MetaRelations_DisplayFieldId",
                table: "MetaRelations",
                column: "DisplayFieldId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "MetaRelations");
            migrationBuilder.DropTable(name: "MetaFields");
            migrationBuilder.DropTable(name: "MetaEntities");
        }
    }
}
