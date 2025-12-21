using B2BCommerce.Backend.Domain.Entities.Integration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace B2BCommerce.Backend.Infrastructure.Data.Configurations.Integration;

/// <summary>
/// EF Core configuration for ApiKeyPermission entity
/// </summary>
public class ApiKeyPermissionConfiguration : IEntityTypeConfiguration<ApiKeyPermission>
{
    public void Configure(EntityTypeBuilder<ApiKeyPermission> builder)
    {
        builder.ToTable("ApiKeyPermissions");

        builder.HasKey(x => x.Id);

        // Match parent's soft delete filter to avoid orphaned navigation issues
        builder.HasQueryFilter(x => !x.ApiKey.IsDeleted);

        builder.Property(x => x.Scope)
            .IsRequired()
            .HasMaxLength(100);

        // Indexes
        builder.HasIndex(x => x.ApiKeyId);
        builder.HasIndex(x => new { x.ApiKeyId, x.Scope }).IsUnique();
    }
}
