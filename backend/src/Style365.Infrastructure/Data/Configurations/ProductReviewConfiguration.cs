using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Style365.Domain.Entities;

namespace Style365.Infrastructure.Data.Configurations;

public class ProductReviewConfiguration : IEntityTypeConfiguration<ProductReview>
{
    public void Configure(EntityTypeBuilder<ProductReview> builder)
    {
        builder.ToTable("ProductReviews");

        builder.HasKey(pr => pr.Id);

        builder.Property(pr => pr.ProductId)
            .IsRequired();

        builder.Property(pr => pr.UserId)
            .IsRequired();

        builder.Property(pr => pr.Rating)
            .IsRequired();

        builder.Property(pr => pr.Title)
            .HasMaxLength(200);

        builder.Property(pr => pr.Comment)
            .HasMaxLength(2000);

        builder.Property(pr => pr.IsVerifiedPurchase)
            .HasDefaultValue(false);

        builder.Property(pr => pr.IsApproved)
            .HasDefaultValue(false);

        builder.Property(pr => pr.ApprovedAt);

        builder.Property(pr => pr.ApprovedBy)
            .HasMaxLength(128);

        builder.Property(pr => pr.CreatedAt)
            .IsRequired();

        builder.Property(pr => pr.UpdatedAt)
            .IsRequired();

        builder.Property(pr => pr.IsDeleted)
            .HasDefaultValue(false);

        // Indexes
        builder.HasIndex(pr => pr.ProductId)
            .HasDatabaseName("IX_ProductReviews_ProductId");

        builder.HasIndex(pr => pr.UserId)
            .HasDatabaseName("IX_ProductReviews_UserId");

        builder.HasIndex(pr => new { pr.ProductId, pr.UserId })
            .HasDatabaseName("IX_ProductReviews_ProductId_UserId");

        builder.HasIndex(pr => new { pr.ProductId, pr.IsApproved })
            .HasDatabaseName("IX_ProductReviews_ProductId_IsApproved");

        // Relationships
        builder.HasOne(pr => pr.Product)
            .WithMany()
            .HasForeignKey(pr => pr.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pr => pr.User)
            .WithMany()
            .HasForeignKey(pr => pr.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}