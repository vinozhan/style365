using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Style365.Domain.Entities;

namespace Style365.Infrastructure.Data.Configurations;

public class ProductTagConfiguration : IEntityTypeConfiguration<ProductTag>
{
    public void Configure(EntityTypeBuilder<ProductTag> builder)
    {
        builder.ToTable("ProductTags");

        builder.HasKey(pt => pt.Id);

        builder.Property(pt => pt.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(pt => pt.Slug)
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(pt => pt.Description)
            .HasMaxLength(500);

        builder.Property(pt => pt.IsActive)
            .HasDefaultValue(true);

        builder.Property(pt => pt.CreatedAt)
            .IsRequired();

        builder.Property(pt => pt.UpdatedAt)
            .IsRequired();

        builder.Property(pt => pt.IsDeleted)
            .HasDefaultValue(false);

        // Indexes
        builder.HasIndex(pt => pt.Name)
            .IsUnique()
            .HasDatabaseName("IX_ProductTags_Name");

        builder.HasIndex(pt => pt.Slug)
            .IsUnique()
            .HasDatabaseName("IX_ProductTags_Slug");

        builder.HasIndex(pt => pt.IsActive)
            .HasDatabaseName("IX_ProductTags_IsActive");
    }
}