using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace B2BCommerce.Backend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddExternalEntityToProductType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExternalCode",
                table: "ProductTypes",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalId",
                table: "ProductTypes",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastSyncedAt",
                table: "ProductTypes",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductTypes_ExternalCode",
                table: "ProductTypes",
                column: "ExternalCode",
                filter: "\"IsDeleted\" = false AND \"ExternalCode\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ProductTypes_ExternalId",
                table: "ProductTypes",
                column: "ExternalId",
                unique: true,
                filter: "\"IsDeleted\" = false AND \"ExternalId\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProductTypes_ExternalCode",
                table: "ProductTypes");

            migrationBuilder.DropIndex(
                name: "IX_ProductTypes_ExternalId",
                table: "ProductTypes");

            migrationBuilder.DropColumn(
                name: "ExternalCode",
                table: "ProductTypes");

            migrationBuilder.DropColumn(
                name: "ExternalId",
                table: "ProductTypes");

            migrationBuilder.DropColumn(
                name: "LastSyncedAt",
                table: "ProductTypes");
        }
    }
}
