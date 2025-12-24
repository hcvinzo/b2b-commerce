using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace B2BCommerce.Backend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProductRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProductRelations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    RelatedProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    RelationType = table.Column<int>(type: "integer", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductRelations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductRelations_Products_RelatedProductId",
                        column: x => x.RelatedProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductRelations_Products_SourceProductId",
                        column: x => x.SourceProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductRelations_DisplayOrder",
                table: "ProductRelations",
                column: "DisplayOrder");

            migrationBuilder.CreateIndex(
                name: "IX_ProductRelations_IsDeleted",
                table: "ProductRelations",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_ProductRelations_RelatedProductId_RelationType",
                table: "ProductRelations",
                columns: new[] { "RelatedProductId", "RelationType" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductRelations_SourceProductId_RelatedProductId_RelationT~",
                table: "ProductRelations",
                columns: new[] { "SourceProductId", "RelatedProductId", "RelationType" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_ProductRelations_SourceProductId_RelationType",
                table: "ProductRelations",
                columns: new[] { "SourceProductId", "RelationType" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductRelations");
        }
    }
}
