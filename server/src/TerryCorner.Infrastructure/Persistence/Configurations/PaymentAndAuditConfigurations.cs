using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TerryCorner.Domain.Entities;

namespace TerryCorner.Infrastructure.Persistence.Configurations;

public class PaymentReceiptConfiguration : IEntityTypeConfiguration<PaymentReceipt>
{
    public void Configure(EntityTypeBuilder<PaymentReceipt> builder)
    {
        builder.Property(r => r.StoredFileName).IsRequired().HasMaxLength(260);
        builder.Property(r => r.OriginalFileNameForDisplay).IsRequired().HasMaxLength(260);
        builder.Property(r => r.ContentType).IsRequired().HasMaxLength(100);

        builder.HasOne(r => r.Order)
            .WithMany(o => o.PaymentReceipts)
            .HasForeignKey(r => r.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(r => r.Status); // powers the admin "Payment Receipts" review queue
    }
}

public class PaymentMethodConfiguration : IEntityTypeConfiguration<PaymentMethod>
{
    public void Configure(EntityTypeBuilder<PaymentMethod> builder)
    {
        builder.Property(m => m.MethodName).IsRequired().HasMaxLength(100);
        builder.Property(m => m.AccountNumberOrIdentifier).IsRequired().HasMaxLength(100);
    }
}

public class PromotionConfiguration : IEntityTypeConfiguration<Promotion>
{
    public void Configure(EntityTypeBuilder<Promotion> builder)
    {
        builder.Property(p => p.Name).IsRequired().HasMaxLength(150);
        builder.Property(p => p.DiscountValue).HasPrecision(10, 2);
        builder.HasIndex(p => new { p.IsActive, p.StartDateUtc, p.EndDateUtc });
    }
}

public class PromotionProductConfiguration : IEntityTypeConfiguration<PromotionProduct>
{
    public void Configure(EntityTypeBuilder<PromotionProduct> builder)
    {
        builder.HasKey(pp => new { pp.PromotionId, pp.ProductId });

        builder.HasOne(pp => pp.Promotion)
            .WithMany(p => p.PromotionProducts)
            .HasForeignKey(pp => pp.PromotionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pp => pp.Product)
            .WithMany()
            .HasForeignKey(pp => pp.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.Property(c => c.FullName).IsRequired().HasMaxLength(150);
        builder.Property(c => c.PhoneNumber).IsRequired().HasMaxLength(30);
        builder.HasIndex(c => c.ApplicationUserId);

        builder.HasMany(c => c.SavedAddresses)
            .WithOne(a => a.Customer)
            .HasForeignKey(a => a.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.Property(t => t.TokenHash).IsRequired().HasMaxLength(200);
        builder.HasIndex(t => t.TokenHash).IsUnique();
        builder.HasIndex(t => t.ApplicationUserId);
    }
}

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.Property(a => a.Action).IsRequired().HasMaxLength(150);
        builder.Property(a => a.EntityType).IsRequired().HasMaxLength(100);
        builder.HasIndex(a => new { a.EntityType, a.EntityId });
    }
}
