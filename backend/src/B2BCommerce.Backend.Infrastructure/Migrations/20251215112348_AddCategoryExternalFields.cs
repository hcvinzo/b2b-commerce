using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace B2BCommerce.Backend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryExternalFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExternalCode",
                table: "Categories",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalId",
                table: "Categories",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastSyncedAt",
                table: "Categories",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Categories_ExternalCode",
                table: "Categories",
                column: "ExternalCode",
                unique: true,
                filter: "\"IsDeleted\" = false AND \"ExternalCode\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_LastSyncedAt",
                table: "Categories",
                column: "LastSyncedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Categories_ExternalCode",
                table: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_Categories_LastSyncedAt",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "ExternalCode",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "ExternalId",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "LastSyncedAt",
                table: "Categories");
        }
    }
}
