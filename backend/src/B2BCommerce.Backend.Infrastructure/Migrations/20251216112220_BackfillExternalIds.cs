using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace B2BCommerce.Backend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class BackfillExternalIds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Backfill ExternalId for entities created before this change
            // ExternalId = Id::text ensures Integration API can reference admin-created entities
            migrationBuilder.Sql(@"
                UPDATE ""Brands"" SET ""ExternalId"" = ""Id""::text WHERE ""ExternalId"" IS NULL;
                UPDATE ""Products"" SET ""ExternalId"" = ""Id""::text WHERE ""ExternalId"" IS NULL;
                UPDATE ""Categories"" SET ""ExternalId"" = ""Id""::text WHERE ""ExternalId"" IS NULL;
                UPDATE ""AttributeDefinitions"" SET ""ExternalId"" = ""Id""::text WHERE ""ExternalId"" IS NULL;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Note: Down migration intentionally does not revert the ExternalId values
            // as this would break Integration API references
        }
    }
}
