using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace B2BCommerce.Backend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddApiClientUserId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "ApiClients",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserId",
                table: "ApiClients");
        }
    }
}
