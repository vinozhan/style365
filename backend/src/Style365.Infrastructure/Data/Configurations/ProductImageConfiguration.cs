using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Style365.Domain.Entities;

namespace Style365.Infrastructure.Data.Configurations;

public class ProductImageConfiguration : IEntityTypeConfiguration<ProductImage>
{
    public void Configure(EntityTypeBuilder<ProductImage> builder)
    {
        builder.ToTable("ProductImages");

        builder.HasKey(pi => pi.Id);

        builder.Property(pi => pi.ProductId)
            .IsRequired();

        builder.Property(pi => pi.ImageUrl)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(pi => pi.AltText)
            .HasMaxLength(300);

        builder.Property(pi => pi.SortOrder)
            .HasDefaultValue(0);

        builder.Property(pi => pi.IsMain)
            .HasDefaultValue(false);

        builder.Property(pi => pi.CreatedAt)
            .IsRequired();

        builder.Property(pi => pi.UpdatedAt)
            .IsRequired();

        builder.Property(pi => pi.IsDeleted)
            .HasDefaultValue(false);

        // Indexes
        builder.HasIndex(pi => pi.ProductId)
            .HasDatabaseName("IX_ProductImages_ProductId");

        builder.HasIndex(pi => new { pi.ProductId, pi.IsMain })
            .HasDatabaseName("IX_ProductImages_ProductId_IsMain");

        builder.HasIndex(pi => new { pi.ProductId, pi.SortOrder })
            .HasDatabaseName("IX_ProductImages_ProductId_SortOrder");
    }
}