using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Style365.Domain.Entities;

namespace Style365.Infrastructure.Data.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(p => p.Slug)
            .HasMaxLength(350)
            .IsRequired();

        builder.Property(p => p.Description)
            .HasMaxLength(5000);

        builder.Property(p => p.ShortDescription)
            .HasMaxLength(500);

        builder.Property(p => p.Sku)
            .HasMaxLength(100)
            .IsRequired();

        builder.OwnsOne(p => p.Price, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("Price")
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            money.Property(m => m.Currency)
                .HasColumnName("Currency")
                .HasMaxLength(3)
                .IsRequired();
        });

        builder.OwnsOne(p => p.ComparePrice, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("ComparePrice")
                .HasColumnType("decimal(18,2)");

            money.Property(m => m.Currency)
                .HasColumnName("ComparePriceCurrency")
                .HasMaxLength(3);
        });

        builder.OwnsOne(p => p.CostPrice, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("CostPrice")
                .HasColumnType("decimal(18,2)");

            money.Property(m => m.Currency)
                .HasColumnName("CostPriceCurrency")
                .HasMaxLength(3);
        });

        builder.Property(p => p.StockQuantity)
            .HasDefaultValue(0);

        builder.Property(p => p.LowStockThreshold)
            .HasDefaultValue(5);

        builder.Property(p => p.TrackQuantity)
            .HasDefaultValue(true);

        builder.Property(p => p.IsActive)
            .HasDefaultValue(true);

        builder.Property(p => p.IsFeatured)
            .HasDefaultValue(false);

        builder.Property(p => p.Weight)
            .HasColumnType("decimal(10,3)")
            .HasDefaultValue(0);

        builder.Property(p => p.WeightUnit)
            .HasMaxLength(10)
            .HasDefaultValue("kg");

        builder.Property(p => p.Brand)
            .HasMaxLength(200);

        builder.Property(p => p.MetaTitle)
            .HasMaxLength(300);

        builder.Property(p => p.MetaDescription)
            .HasMaxLength(500);

        builder.Property(p => p.CategoryId)
            .IsRequired();

        builder.Property(p => p.CreatedAt)
            .IsRequired();

        builder.Property(p => p.UpdatedAt)
            .IsRequired();

        builder.Property(p => p.CreatedBy)
            .HasMaxLength(128);

        builder.Property(p => p.UpdatedBy)
            .HasMaxLength(128);

        builder.Property(p => p.IsDeleted)
            .HasDefaultValue(false);

        // Indexes
        builder.HasIndex(p => p.Slug)
            .IsUnique()
            .HasDatabaseName("IX_Products_Slug");

        builder.HasIndex(p => p.Sku)
            .IsUnique()
            .HasDatabaseName("IX_Products_Sku");

        builder.HasIndex(p => p.Name)
            .HasDatabaseName("IX_Products_Name");

        builder.HasIndex(p => p.CategoryId)
            .HasDatabaseName("IX_Products_CategoryId");

        builder.HasIndex(p => p.Brand)
            .HasDatabaseName("IX_Products_Brand");

        builder.HasIndex(p => new { p.IsActive, p.IsFeatured })
            .HasDatabaseName("IX_Products_IsActive_IsFeatured");

        builder.HasIndex(p => new { p.IsActive, p.CategoryId })
            .HasDatabaseName("IX_Products_IsActive_CategoryId");

        // Relationships
        builder.HasMany(p => p.Images)
            .WithOne(i => i.Product)
            .HasForeignKey(i => i.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Variants)
            .WithOne(v => v.Product)
            .HasForeignKey(v => v.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}