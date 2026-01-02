using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace B2BCommerce.Backend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCampaignDiscountFeature : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Campaigns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Priority = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    TotalBudgetLimitAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    TotalBudgetLimitCurrency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    TotalUsageLimit = table.Column<int>(type: "integer", nullable: true),
                    PerCustomerBudgetLimitAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    PerCustomerBudgetLimitCurrency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    PerCustomerUsageLimit = table.Column<int>(type: "integer", nullable: true),
                    TotalDiscountUsedAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TotalDiscountUsedCurrency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    TotalUsageCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ExternalCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ExternalId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    LastSyncedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Campaigns", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CampaignUsages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CampaignId = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderItemId = table.Column<Guid>(type: "uuid", nullable: true),
                    DiscountAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    DiscountCurrency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    UsedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsReversed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ReversedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignUsages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CampaignUsages_Campaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CampaignUsages_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CampaignUsages_OrderItems_OrderItemId",
                        column: x => x.OrderItemId,
                        principalTable: "OrderItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CampaignUsages_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DiscountRules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CampaignId = table.Column<Guid>(type: "uuid", nullable: false),
                    DiscountType = table.Column<int>(type: "integer", nullable: false),
                    DiscountValue = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    MaxDiscountAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    ProductTargetType = table.Column<int>(type: "integer", nullable: false),
                    CustomerTargetType = table.Column<int>(type: "integer", nullable: false),
                    MinOrderAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    MinQuantity = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscountRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DiscountRules_Campaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DiscountRuleBrands",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DiscountRuleId = table.Column<Guid>(type: "uuid", nullable: false),
                    BrandId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscountRuleBrands", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DiscountRuleBrands_Brands_BrandId",
                        column: x => x.BrandId,
                        principalTable: "Brands",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DiscountRuleBrands_DiscountRules_DiscountRuleId",
                        column: x => x.DiscountRuleId,
                        principalTable: "DiscountRules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DiscountRuleCategories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DiscountRuleId = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscountRuleCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DiscountRuleCategories_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DiscountRuleCategories_DiscountRules_DiscountRuleId",
                        column: x => x.DiscountRuleId,
                        principalTable: "DiscountRules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DiscountRuleCustomers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DiscountRuleId = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscountRuleCustomers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DiscountRuleCustomers_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DiscountRuleCustomers_DiscountRules_DiscountRuleId",
                        column: x => x.DiscountRuleId,
                        principalTable: "DiscountRules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DiscountRuleCustomerTiers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DiscountRuleId = table.Column<Guid>(type: "uuid", nullable: false),
                    PriceTier = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscountRuleCustomerTiers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DiscountRuleCustomerTiers_DiscountRules_DiscountRuleId",
                        column: x => x.DiscountRuleId,
                        principalTable: "DiscountRules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DiscountRuleProducts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DiscountRuleId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscountRuleProducts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DiscountRuleProducts_DiscountRules_DiscountRuleId",
                        column: x => x.DiscountRuleId,
                        principalTable: "DiscountRules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DiscountRuleProducts_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Campaigns_EndDate",
                table: "Campaigns",
                column: "EndDate");

            migrationBuilder.CreateIndex(
                name: "IX_Campaigns_ExternalCode",
                table: "Campaigns",
                column: "ExternalCode",
                filter: "\"IsDeleted\" = false AND \"ExternalCode\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Campaigns_ExternalId",
                table: "Campaigns",
                column: "ExternalId",
                unique: true,
                filter: "\"IsDeleted\" = false AND \"ExternalId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Campaigns_IsDeleted",
                table: "Campaigns",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Campaigns_LastSyncedAt",
                table: "Campaigns",
                column: "LastSyncedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Campaigns_Name",
                table: "Campaigns",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Campaigns_Priority",
                table: "Campaigns",
                column: "Priority");

            migrationBuilder.CreateIndex(
                name: "IX_Campaigns_StartDate",
                table: "Campaigns",
                column: "StartDate");

            migrationBuilder.CreateIndex(
                name: "IX_Campaigns_Status",
                table: "Campaigns",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Campaigns_Status_StartDate_EndDate",
                table: "Campaigns",
                columns: new[] { "Status", "StartDate", "EndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_CampaignUsages_CampaignId",
                table: "CampaignUsages",
                column: "CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignUsages_CampaignId_CustomerId",
                table: "CampaignUsages",
                columns: new[] { "CampaignId", "CustomerId" });

            migrationBuilder.CreateIndex(
                name: "IX_CampaignUsages_CustomerId",
                table: "CampaignUsages",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignUsages_IsDeleted",
                table: "CampaignUsages",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignUsages_IsReversed",
                table: "CampaignUsages",
                column: "IsReversed");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignUsages_OrderId",
                table: "CampaignUsages",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignUsages_OrderItemId",
                table: "CampaignUsages",
                column: "OrderItemId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignUsages_UsedAt",
                table: "CampaignUsages",
                column: "UsedAt");

            migrationBuilder.CreateIndex(
                name: "IX_DiscountRuleBrands_BrandId",
                table: "DiscountRuleBrands",
                column: "BrandId");

            migrationBuilder.CreateIndex(
                name: "IX_DiscountRuleBrands_DiscountRuleId",
                table: "DiscountRuleBrands",
                column: "DiscountRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_DiscountRuleBrands_DiscountRuleId_BrandId",
                table: "DiscountRuleBrands",
                columns: new[] { "DiscountRuleId", "BrandId" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_DiscountRuleBrands_IsDeleted",
                table: "DiscountRuleBrands",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_DiscountRuleCategories_CategoryId",
                table: "DiscountRuleCategories",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_DiscountRuleCategories_DiscountRuleId",
                table: "DiscountRuleCategories",
                column: "DiscountRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_DiscountRuleCategories_DiscountRuleId_CategoryId",
                table: "DiscountRuleCategories",
                columns: new[] { "DiscountRuleId", "CategoryId" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_DiscountRuleCategories_IsDeleted",
                table: "DiscountRuleCategories",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_DiscountRuleCustomers_CustomerId",
                table: "DiscountRuleCustomers",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_DiscountRuleCustomers_DiscountRuleId",
                table: "DiscountRuleCustomers",
                column: "DiscountRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_DiscountRuleCustomers_DiscountRuleId_CustomerId",
                table: "DiscountRuleCustomers",
                columns: new[] { "DiscountRuleId", "CustomerId" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_DiscountRuleCustomers_IsDeleted",
                table: "DiscountRuleCustomers",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_DiscountRuleCustomerTiers_DiscountRuleId",
                table: "DiscountRuleCustomerTiers",
                column: "DiscountRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_DiscountRuleCustomerTiers_DiscountRuleId_PriceTier",
                table: "DiscountRuleCustomerTiers",
                columns: new[] { "DiscountRuleId", "PriceTier" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_DiscountRuleCustomerTiers_IsDeleted",
                table: "DiscountRuleCustomerTiers",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_DiscountRuleCustomerTiers_PriceTier",
                table: "DiscountRuleCustomerTiers",
                column: "PriceTier");

            migrationBuilder.CreateIndex(
                name: "IX_DiscountRuleProducts_DiscountRuleId",
                table: "DiscountRuleProducts",
                column: "DiscountRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_DiscountRuleProducts_DiscountRuleId_ProductId",
                table: "DiscountRuleProducts",
                columns: new[] { "DiscountRuleId", "ProductId" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_DiscountRuleProducts_IsDeleted",
                table: "DiscountRuleProducts",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_DiscountRuleProducts_ProductId",
                table: "DiscountRuleProducts",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_DiscountRules_CampaignId",
                table: "DiscountRules",
                column: "CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_DiscountRules_CustomerTargetType",
                table: "DiscountRules",
                column: "CustomerTargetType");

            migrationBuilder.CreateIndex(
                name: "IX_DiscountRules_DiscountType",
                table: "DiscountRules",
                column: "DiscountType");

            migrationBuilder.CreateIndex(
                name: "IX_DiscountRules_IsDeleted",
                table: "DiscountRules",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_DiscountRules_ProductTargetType",
                table: "DiscountRules",
                column: "ProductTargetType");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CampaignUsages");

            migrationBuilder.DropTable(
                name: "DiscountRuleBrands");

            migrationBuilder.DropTable(
                name: "DiscountRuleCategories");

            migrationBuilder.DropTable(
                name: "DiscountRuleCustomers");

            migrationBuilder.DropTable(
                name: "DiscountRuleCustomerTiers");

            migrationBuilder.DropTable(
                name: "DiscountRuleProducts");

            migrationBuilder.DropTable(
                name: "DiscountRules");

            migrationBuilder.DropTable(
                name: "Campaigns");
        }
    }
}
