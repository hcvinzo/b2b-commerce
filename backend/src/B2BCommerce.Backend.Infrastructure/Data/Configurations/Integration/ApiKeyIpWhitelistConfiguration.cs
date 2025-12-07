using B2BCommerce.Backend.Domain.Entities.Integration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace B2BCommerce.Backend.Infrastructure.Data.Configurations.Integration;

/// <summary>
/// EF Core configuration for ApiKeyIpWhitelist entity
/// </summary>
public class ApiKeyIpWhitelistConfiguration : IEntityTypeConfiguration<ApiKeyIpWhitelist>
{
    public void Configure(EntityTypeBuilder<ApiKeyIpWhitelist> builder)
    {
        builder.ToTable("ApiKeyIpWhitelist");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.IpAddress)
            .IsRequired()
            .HasMaxLength(45);

        builder.Property(x => x.Description)
            .HasMaxLength(200);

        // Indexes
        builder.HasIndex(x => x.ApiKeyId);
    }
}
