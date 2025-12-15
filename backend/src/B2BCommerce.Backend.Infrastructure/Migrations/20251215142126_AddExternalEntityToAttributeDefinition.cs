using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace B2BCommerce.Backend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddExternalEntityToAttributeDefinition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExternalCode",
                table: "AttributeDefinitions",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalId",
                table: "AttributeDefinitions",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastSyncedAt",
                table: "AttributeDefinitions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AttributeDefinitions_ExternalCode",
                table: "AttributeDefinitions",
                column: "ExternalCode",
                filter: "\"IsDeleted\" = false AND \"ExternalCode\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AttributeDefinitions_ExternalId",
                table: "AttributeDefinitions",
                column: "ExternalId",
                unique: true,
                filter: "\"IsDeleted\" = false AND \"ExternalId\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AttributeDefinitions_ExternalCode",
                table: "AttributeDefinitions");

            migrationBuilder.DropIndex(
                name: "IX_AttributeDefinitions_ExternalId",
                table: "AttributeDefinitions");

            migrationBuilder.DropColumn(
                name: "ExternalCode",
                table: "AttributeDefinitions");

            migrationBuilder.DropColumn(
                name: "ExternalId",
                table: "AttributeDefinitions");

            migrationBuilder.DropColumn(
                name: "LastSyncedAt",
                table: "AttributeDefinitions");
        }
    }
}
