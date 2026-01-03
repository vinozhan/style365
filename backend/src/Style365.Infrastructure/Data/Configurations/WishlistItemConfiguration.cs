using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Style365.Domain.Entities;

namespace Style365.Infrastructure.Data.Configurations;

public class WishlistItemConfiguration : IEntityTypeConfiguration<WishlistItem>
{
    public void Configure(EntityTypeBuilder<WishlistItem> builder)
    {
        builder.ToTable("WishlistItems");

        builder.HasKey(wi => wi.Id);

        builder.Property(wi => wi.WishlistId)
            .IsRequired();

        builder.Property(wi => wi.ProductId)
            .IsRequired();

        builder.Property(wi => wi.AddedAt)
            .IsRequired();

        builder.Property(wi => wi.CreatedAt)
            .IsRequired();

        builder.Property(wi => wi.UpdatedAt)
            .IsRequired();

        builder.Property(wi => wi.IsDeleted)
            .HasDefaultValue(false);

        // Indexes
        builder.HasIndex(wi => wi.WishlistId)
            .HasDatabaseName("IX_WishlistItems_WishlistId");

        builder.HasIndex(wi => wi.ProductId)
            .HasDatabaseName("IX_WishlistItems_ProductId");

        builder.HasIndex(wi => new { wi.WishlistId, wi.ProductId })
            .IsUnique()
            .HasDatabaseName("IX_WishlistItems_WishlistId_ProductId");

        // Relationships
        builder.HasOne(wi => wi.Product)
            .WithMany()
            .HasForeignKey(wi => wi.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}