using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TerryCorner.Domain.Entities;

namespace TerryCorner.Infrastructure.Persistence.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.Property(c => c.Name).IsRequired().HasMaxLength(100);
        builder.HasIndex(c => c.SortOrder);
    }
}

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.Property(p => p.Name).IsRequired().HasMaxLength(150);
        builder.Property(p => p.Price).HasPrecision(10, 2);

        builder.HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => new { p.CategoryId, p.IsAvailable });
        builder.HasIndex(p => p.IsFeatured);
    }
}

public class ToppingConfiguration : IEntityTypeConfiguration<Topping>
{
    public void Configure(EntityTypeBuilder<Topping> builder)
    {
        builder.Property(t => t.Name).IsRequired().HasMaxLength(100);
        builder.Property(t => t.AdditionalPrice).HasPrecision(10, 2);
    }
}

public class ProductToppingConfiguration : IEntityTypeConfiguration<ProductTopping>
{
    public void Configure(EntityTypeBuilder<ProductTopping> builder)
    {
        builder.HasKey(pt => new { pt.ProductId, pt.ToppingId });

        builder.HasOne(pt => pt.Product)
            .WithMany(p => p.ProductToppings)
            .HasForeignKey(pt => pt.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pt => pt.Topping)
            .WithMany(t => t.ProductToppings)
            .HasForeignKey(pt => pt.ToppingId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
