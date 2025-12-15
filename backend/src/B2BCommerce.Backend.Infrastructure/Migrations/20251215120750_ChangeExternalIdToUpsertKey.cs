using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace B2BCommerce.Backend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ChangeExternalIdToUpsertKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Categories_ExternalCode",
                table: "Categories");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_ExternalCode",
                table: "Categories",
                column: "ExternalCode",
                filter: "\"IsDeleted\" = false AND \"ExternalCode\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_ExternalId",
                table: "Categories",
                column: "ExternalId",
                unique: true,
                filter: "\"IsDeleted\" = false AND \"ExternalId\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Categories_ExternalCode",
                table: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_Categories_ExternalId",
                table: "Categories");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_ExternalCode",
                table: "Categories",
                column: "ExternalCode",
                unique: true,
                filter: "\"IsDeleted\" = false AND \"ExternalCode\" IS NOT NULL");
        }
    }
}
