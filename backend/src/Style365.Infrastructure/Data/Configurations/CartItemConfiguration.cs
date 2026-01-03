using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Style365.Domain.Entities;

namespace Style365.Infrastructure.Data.Configurations;

public class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        builder.ToTable("CartItems");

        builder.HasKey(ci => ci.Id);

        builder.Property(ci => ci.CartId)
            .IsRequired();

        builder.Property(ci => ci.ProductId)
            .IsRequired();

        builder.Property(ci => ci.ProductVariantId);

        builder.Property(ci => ci.Quantity)
            .IsRequired();

        builder.Property(ci => ci.AddedAt)
            .IsRequired();

        builder.Property(ci => ci.CreatedAt)
            .IsRequired();

        builder.Property(ci => ci.UpdatedAt)
            .IsRequired();

        builder.Property(ci => ci.IsDeleted)
            .HasDefaultValue(false);

        // Indexes
        builder.HasIndex(ci => ci.CartId)
            .HasDatabaseName("IX_CartItems_CartId");

        builder.HasIndex(ci => ci.ProductId)
            .HasDatabaseName("IX_CartItems_ProductId");

        builder.HasIndex(ci => new { ci.CartId, ci.ProductId, ci.ProductVariantId })
            .IsUnique()
            .HasDatabaseName("IX_CartItems_CartId_ProductId_VariantId");

        // Relationships
        builder.HasOne(ci => ci.Product)
            .WithMany()
            .HasForeignKey(ci => ci.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ci => ci.ProductVariant)
            .WithMany()
            .HasForeignKey(ci => ci.ProductVariantId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}