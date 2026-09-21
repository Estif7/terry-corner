using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TerryCorner.Domain.Entities;

namespace TerryCorner.Infrastructure.Persistence.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.Property(o => o.OrderNumber).IsRequired().HasMaxLength(20);
        builder.HasIndex(o => o.OrderNumber).IsUnique();

        builder.Property(o => o.Subtotal).HasPrecision(10, 2);
        builder.Property(o => o.DiscountAmount).HasPrecision(10, 2);
        builder.Property(o => o.Total).HasPrecision(10, 2);

        builder.HasOne(o => o.Customer)
            .WithMany(c => c.Orders)
            .HasForeignKey(o => o.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.Promotion)
            .WithMany()
            .HasForeignKey(o => o.PromotionId)
            .OnDelete(DeleteBehavior.SetNull);

        // Supports admin order list filtering/sorting by status, and the customer's own order history.
        builder.HasIndex(o => o.Status);
        builder.HasIndex(o => new { o.CustomerId, o.CreatedAtUtc });
    }
}

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.Property(i => i.ProductNameSnapshot).IsRequired().HasMaxLength(150);
        builder.Property(i => i.UnitPriceSnapshot).HasPrecision(10, 2);
        builder.Property(i => i.LineTotal).HasPrecision(10, 2);

        builder.HasOne(i => i.Order)
            .WithMany(o => o.Items)
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(i => i.Product)
            .WithMany()
            .HasForeignKey(i => i.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class OrderItemToppingConfiguration : IEntityTypeConfiguration<OrderItemTopping>
{
    public void Configure(EntityTypeBuilder<OrderItemTopping> builder)
    {
        builder.HasKey(t => new { t.OrderItemId, t.ToppingId });

        builder.Property(t => t.ToppingNameSnapshot).IsRequired().HasMaxLength(100);
        builder.Property(t => t.AdditionalPriceSnapshot).HasPrecision(10, 2);

        builder.HasOne(t => t.OrderItem)
            .WithMany(i => i.Toppings)
            .HasForeignKey(t => t.OrderItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(t => t.Topping)
            .WithMany()
            .HasForeignKey(t => t.ToppingId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class OrderStatusHistoryConfiguration : IEntityTypeConfiguration<OrderStatusHistory>
{
    public void Configure(EntityTypeBuilder<OrderStatusHistory> builder)
    {
        builder.HasOne(h => h.Order)
            .WithMany(o => o.StatusHistory)
            .HasForeignKey(h => h.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(h => h.OrderId);
    }
}
