using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace B2BCommerce.Backend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddParameterTypeAndValueTypeToSystemConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ParameterType",
                table: "SystemConfigurations",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ValueType",
                table: "SystemConfigurations",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_SystemConfigurations_ParameterType",
                table: "SystemConfigurations",
                column: "ParameterType");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SystemConfigurations_ParameterType",
                table: "SystemConfigurations");

            migrationBuilder.DropColumn(
                name: "ParameterType",
                table: "SystemConfigurations");

            migrationBuilder.DropColumn(
                name: "ValueType",
                table: "SystemConfigurations");
        }
    }
}
