using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Style365.Domain.Entities;

namespace Style365.Infrastructure.Data.Configurations;

public class UserAddressConfiguration : IEntityTypeConfiguration<UserAddress>
{
    public void Configure(EntityTypeBuilder<UserAddress> builder)
    {
        builder.ToTable("UserAddresses");

        builder.HasKey(ua => ua.Id);

        builder.Property(ua => ua.UserId)
            .IsRequired();

        builder.OwnsOne(ua => ua.Address, address =>
        {
            address.Property(a => a.AddressLine1)
                .HasColumnName("AddressLine1")
                .HasMaxLength(200)
                .IsRequired();

            address.Property(a => a.AddressLine2)
                .HasColumnName("AddressLine2")
                .HasMaxLength(200);

            address.Property(a => a.City)
                .HasColumnName("City")
                .HasMaxLength(100)
                .IsRequired();

            address.Property(a => a.State)
                .HasColumnName("State")
                .HasMaxLength(100)
                .IsRequired();

            address.Property(a => a.PostalCode)
                .HasColumnName("PostalCode")
                .HasMaxLength(20)
                .IsRequired();

            address.Property(a => a.Country)
                .HasColumnName("Country")
                .HasMaxLength(100)
                .IsRequired();
        });

        builder.Property(ua => ua.IsDefault)
            .HasDefaultValue(false);

        builder.Property(ua => ua.Label)
            .HasMaxLength(50);

        builder.Property(ua => ua.CreatedAt)
            .IsRequired();

        builder.Property(ua => ua.UpdatedAt)
            .IsRequired();

        builder.Property(ua => ua.CreatedBy)
            .HasMaxLength(128);

        builder.Property(ua => ua.UpdatedBy)
            .HasMaxLength(128);

        builder.Property(ua => ua.IsDeleted)
            .HasDefaultValue(false);

        // Indexes
        builder.HasIndex(ua => ua.UserId)
            .HasDatabaseName("IX_UserAddresses_UserId");

        builder.HasIndex(ua => new { ua.UserId, ua.IsDefault })
            .HasDatabaseName("IX_UserAddresses_UserId_IsDefault");
    }
}