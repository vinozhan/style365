using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Style365.Domain.Entities;

namespace Style365.Infrastructure.Data.Configurations;

public class ProductVariantConfiguration : IEntityTypeConfiguration<ProductVariant>
{
    public void Configure(EntityTypeBuilder<ProductVariant> builder)
    {
        builder.ToTable("ProductVariants");

        builder.HasKey(pv => pv.Id);

        builder.Property(pv => pv.ProductId)
            .IsRequired();

        builder.Property(pv => pv.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(pv => pv.Sku)
            .HasMaxLength(100)
            .IsRequired();

        builder.OwnsOne(pv => pv.Price, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("Price")
                .HasColumnType("decimal(18,2)");

            money.Property(m => m.Currency)
                .HasColumnName("Currency")
                .HasMaxLength(3);
        });

        builder.OwnsOne(pv => pv.ComparePrice, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("ComparePrice")
                .HasColumnType("decimal(18,2)");

            money.Property(m => m.Currency)
                .HasColumnName("ComparePriceCurrency")
                .HasMaxLength(3);
        });

        builder.Property(pv => pv.StockQuantity)
            .HasDefaultValue(0);

        builder.Property(pv => pv.TrackQuantity)
            .HasDefaultValue(true);

        builder.Property(pv => pv.IsActive)
            .HasDefaultValue(true);

        builder.Property(pv => pv.Color)
            .HasMaxLength(50);

        builder.Property(pv => pv.Size)
            .HasMaxLength(50);

        builder.Property(pv => pv.Material)
            .HasMaxLength(100);

        builder.Property(pv => pv.Weight)
            .HasColumnType("decimal(10,3)");

        builder.Property(pv => pv.ImageUrl)
            .HasMaxLength(1000);

        builder.Property(pv => pv.CreatedAt)
            .IsRequired();

        builder.Property(pv => pv.UpdatedAt)
            .IsRequired();

        builder.Property(pv => pv.IsDeleted)
            .HasDefaultValue(false);

        // Indexes
        builder.HasIndex(pv => pv.Sku)
            .IsUnique()
            .HasDatabaseName("IX_ProductVariants_Sku");

        builder.HasIndex(pv => pv.ProductId)
            .HasDatabaseName("IX_ProductVariants_ProductId");

        builder.HasIndex(pv => new { pv.ProductId, pv.IsActive })
            .HasDatabaseName("IX_ProductVariants_ProductId_IsActive");
    }
}