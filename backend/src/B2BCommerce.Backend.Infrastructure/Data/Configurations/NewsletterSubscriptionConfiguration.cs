using B2BCommerce.Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace B2BCommerce.Backend.Infrastructure.Data.Configurations;

public class NewsletterSubscriptionConfiguration : IEntityTypeConfiguration<NewsletterSubscription>
{
    public void Configure(EntityTypeBuilder<NewsletterSubscription> builder)
    {
        builder.ToTable("NewsletterSubscriptions");

        builder.HasKey(n => n.Id);

        // Email value object conversion
        builder.Property(n => n.Email)
            .HasConversion(
                v => v.Value,
                v => new Domain.ValueObjects.Email(v))
            .HasColumnName("Email")
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(n => n.SubscribedAt)
            .IsRequired();

        builder.Property(n => n.IsVerified)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(n => n.VerifiedAt);

        builder.Property(n => n.UnsubscribedAt);

        // Audit properties
        builder.Property(n => n.CreatedAt)
            .IsRequired();

        builder.Property(n => n.CreatedBy)
            .HasMaxLength(100);

        builder.Property(n => n.UpdatedAt);

        builder.Property(n => n.UpdatedBy)
            .HasMaxLength(100);

        builder.Property(n => n.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(n => n.DeletedAt);

        builder.Property(n => n.DeletedBy)
            .HasMaxLength(100);

        // Indexes
        builder.HasIndex(n => n.Email)
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");

        builder.HasIndex(n => n.IsDeleted);

        builder.HasIndex(n => n.IsVerified);
    }
}
