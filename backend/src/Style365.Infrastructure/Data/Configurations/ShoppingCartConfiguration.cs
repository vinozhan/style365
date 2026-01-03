using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Style365.Domain.Entities;

namespace Style365.Infrastructure.Data.Configurations;

public class ShoppingCartConfiguration : IEntityTypeConfiguration<ShoppingCart>
{
    public void Configure(EntityTypeBuilder<ShoppingCart> builder)
    {
        builder.ToTable("ShoppingCarts");

        builder.HasKey(sc => sc.Id);

        builder.Property(sc => sc.UserId);

        builder.Property(sc => sc.SessionId)
            .HasMaxLength(128);

        builder.Property(sc => sc.LastModified)
            .IsRequired();

        builder.Property(sc => sc.ExpiresAt);

        builder.Property(sc => sc.CreatedAt)
            .IsRequired();

        builder.Property(sc => sc.UpdatedAt)
            .IsRequired();

        builder.Property(sc => sc.IsDeleted)
            .HasDefaultValue(false);

        // Indexes
        builder.HasIndex(sc => sc.UserId)
            .HasDatabaseName("IX_ShoppingCarts_UserId");

        builder.HasIndex(sc => sc.SessionId)
            .HasDatabaseName("IX_ShoppingCarts_SessionId");

        builder.HasIndex(sc => sc.ExpiresAt)
            .HasDatabaseName("IX_ShoppingCarts_ExpiresAt");

        // Relationships
        builder.HasMany(sc => sc.Items)
            .WithOne(ci => ci.Cart)
            .HasForeignKey(ci => ci.CartId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}