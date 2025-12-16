using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace B2BCommerce.Backend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProductStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Add Status column with default value (Draft = 0)
            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Products",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            // 2. Migrate existing data: IsActive=true -> Status=1 (Active), IsActive=false -> Status=2 (Inactive)
            // Products that were active become Active, products that were inactive become Inactive (not Draft)
            migrationBuilder.Sql("""
                UPDATE "Products" SET "Status" = CASE
                    WHEN "IsActive" = true THEN 1  -- Active
                    ELSE 2                          -- Inactive
                END;
            """);

            // 3. Drop old IsActive column and index
            migrationBuilder.DropIndex(
                name: "IX_Products_IsActive",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Products");

            // 4. Create new index on Status
            migrationBuilder.CreateIndex(
                name: "IX_Products_Status",
                table: "Products",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // 1. Add back IsActive column
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Products",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            // 2. Migrate data back: Status=1 (Active) -> IsActive=true, anything else -> IsActive=false
            migrationBuilder.Sql("""
                UPDATE "Products" SET "IsActive" = CASE
                    WHEN "Status" = 1 THEN true  -- Active
                    ELSE false                    -- Draft or Inactive
                END;
            """);

            // 3. Drop Status column and index
            migrationBuilder.DropIndex(
                name: "IX_Products_Status",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Products");

            // 4. Create index on IsActive
            migrationBuilder.CreateIndex(
                name: "IX_Products_IsActive",
                table: "Products",
                column: "IsActive");
        }
    }
}
