using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace B2BCommerce.Backend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserTypeToApplicationUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add UserType column with default value 0 (Admin)
            migrationBuilder.AddColumn<int>(
                name: "UserType",
                table: "Users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            // Set UserType to Customer (1) for users that have a CustomerId
            migrationBuilder.Sql(
                @"UPDATE ""Users"" SET ""UserType"" = 1 WHERE ""CustomerId"" IS NOT NULL");

            // Set UserType to ApiClient (2) for users created by API clients
            // These are identified by being linked to an ApiClient via UserId
            // UserId in ApiClients is text, Id in Users is uuid, so we need to cast
            migrationBuilder.Sql(
                @"UPDATE ""Users"" SET ""UserType"" = 2
                  WHERE ""Id""::text IN (SELECT ""UserId"" FROM ""ApiClients"" WHERE ""UserId"" IS NOT NULL)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserType",
                table: "Users");
        }
    }
}
