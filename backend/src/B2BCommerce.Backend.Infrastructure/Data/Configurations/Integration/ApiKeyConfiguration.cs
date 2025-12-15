using B2BCommerce.Backend.Domain.Entities.Integration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace B2BCommerce.Backend.Infrastructure.Data.Configurations.Integration;

/// <summary>
/// EF Core configuration for ApiKey entity
/// </summary>
public class ApiKeyConfiguration : IEntityTypeConfiguration<ApiKey>
{
    public void Configure(EntityTypeBuilder<ApiKey> builder)
    {
        builder.ToTable("ApiKeys");

        builder.HasKey(x => x.Id);

        // Global soft delete filter
        builder.HasQueryFilter(x => !x.IsDeleted);

        builder.Property(x => x.KeyHash)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(x => x.KeyPrefix)
            .IsRequired()
            .HasMaxLength(16);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.RateLimitPerMinute)
            .IsRequired()
            .HasDefaultValue(500);

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(x => x.LastUsedIp)
            .HasMaxLength(45);

        builder.Property(x => x.RevokedBy)
            .HasMaxLength(100);

        builder.Property(x => x.RevocationReason)
            .HasMaxLength(500);

        builder.Property(x => x.CreatedBy)
            .HasMaxLength(100);

        // Indexes
        builder.HasIndex(x => x.KeyHash);
        builder.HasIndex(x => x.KeyPrefix);
        builder.HasIndex(x => x.ApiClientId);
        builder.HasIndex(x => new { x.IsActive, x.ExpiresAt });

        // Relationships
        builder.HasMany(x => x.Permissions)
            .WithOne(x => x.ApiKey)
            .HasForeignKey(x => x.ApiKeyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.IpWhitelist)
            .WithOne(x => x.ApiKey)
            .HasForeignKey(x => x.ApiKeyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
