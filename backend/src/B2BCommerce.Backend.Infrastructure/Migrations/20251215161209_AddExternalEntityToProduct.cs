using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace B2BCommerce.Backend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddExternalEntityToProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExternalCode",
                table: "Products",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalId",
                table: "Products",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastSyncedAt",
                table: "Products",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_ExternalCode",
                table: "Products",
                column: "ExternalCode",
                filter: "\"IsDeleted\" = false AND \"ExternalCode\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Products_ExternalId",
                table: "Products",
                column: "ExternalId",
                unique: true,
                filter: "\"IsDeleted\" = false AND \"ExternalId\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Products_ExternalCode",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_ExternalId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ExternalCode",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ExternalId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "LastSyncedAt",
                table: "Products");
        }
    }
}
