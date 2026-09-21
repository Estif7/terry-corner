using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TerryCorner.Domain.Entities;
using TerryCorner.Infrastructure.Identity;

namespace TerryCorner.Infrastructure.Persistence;

/// <summary>Dev/staging convenience seeder. Never run automatically in Production.</summary>
public static class DbSeeder
{
    public static async Task SeedAsync(
        TerryCornerDbContext db,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        foreach (var role in Roles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        const string adminEmail = "admin@terrycorner.local";
        if (await userManager.FindByEmailAsync(adminEmail) is null)
        {
            var admin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FullName = "Terry Corner Admin",
                EmailConfirmed = true,
                CreatedAtUtc = DateTime.UtcNow,
            };
            // Dev-only seed password — rotate immediately in any shared environment.
            var result = await userManager.CreateAsync(admin, "ChangeMe123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, Roles.Admin);
            }
        }

        if (await db.Categories.AnyAsync())
        {
            return; // catalog already seeded
        }

        var burgers = new Category { Name = "Burgers", SortOrder = 1 };
        var chicken = new Category { Name = "Chicken", SortOrder = 2 };
        var combos = new Category { Name = "Combos", SortOrder = 3 };
        var fries = new Category { Name = "Fries", SortOrder = 4 };
        var drinks = new Category { Name = "Drinks", SortOrder = 5 };
        db.Categories.AddRange(burgers, chicken, combos, fries, drinks);

        var extraCheese = new Topping { Name = "Extra Cheese", AdditionalPrice = 50, SortOrder = 1 };
        var bacon = new Topping { Name = "Bacon", AdditionalPrice = 100, SortOrder = 2 };
        var jalapeno = new Topping { Name = "Jalapeño", AdditionalPrice = 30, SortOrder = 3 };
        var extraBeef = new Topping { Name = "Extra Beef Patty", AdditionalPrice = 200, SortOrder = 4 };
        var pickles = new Topping { Name = "Pickles", AdditionalPrice = 20, SortOrder = 5 };
        var onions = new Topping { Name = "Onions", AdditionalPrice = 20, SortOrder = 6 };
        var specialSauce = new Topping { Name = "Special Sauce", AdditionalPrice = 30, SortOrder = 7 };
        db.Toppings.AddRange(extraCheese, bacon, jalapeno, extraBeef, pickles, onions, specialSauce);

        var burgerToppings = new[] { extraCheese, bacon, jalapeno, extraBeef, pickles, onions, specialSauce };

        var classic = new Product
        {
            Name = "Terry's Classic",
            Description = "Flame-grilled beef patty, lettuce, tomato, house sauce, toasted bun.",
            Price = 450,
            Category = burgers,
            IsFeatured = true,
            IsPopular = true,
            SortOrder = 1,
        };
        var doubleBurger = new Product
        {
            Name = "Terry's Double",
            Description = "Two beef patties, double cheese, pickles, onions, special sauce.",
            Price = 650,
            Category = burgers,
            IsFeatured = true,
            SortOrder = 2,
        };
        var fireBurger = new Product
        {
            Name = "Fire Burger",
            Description = "Spicy jalapeño, pepper jack, chipotle mayo, crispy onions.",
            Price = 550,
            Category = burgers,
            IsPopular = true,
            SortOrder = 3,
        };
        var crispyChicken = new Product
        {
            Name = "Crispy Chicken",
            Description = "Buttermilk-fried chicken breast, slaw, pickles, garlic aioli.",
            Price = 500,
            Category = chicken,
            IsFeatured = true,
            SortOrder = 1,
        };
        var bbqBacon = new Product
        {
            Name = "BBQ Bacon Burger",
            Description = "Smoky BBQ sauce, crispy bacon, cheddar, onion rings.",
            Price = 600,
            Category = burgers,
            IsPopular = true,
            SortOrder = 4,
        };
        db.Products.AddRange(classic, doubleBurger, fireBurger, crispyChicken, bbqBacon);

        foreach (var product in new[] { classic, doubleBurger, fireBurger, bbqBacon })
        {
            foreach (var topping in burgerToppings)
            {
                db.ProductToppings.Add(new ProductTopping { Product = product, Topping = topping });
            }
        }

        var friesProduct = new Product
        {
            Name = "Classic Fries",
            Description = "Golden, crispy, lightly salted.",
            Price = 150,
            Category = fries,
            SortOrder = 1,
        };
        var softDrink = new Product
        {
            Name = "Soft Drink",
            Description = "Ice-cold cola, orange, or lemon-lime.",
            Price = 80,
            Category = drinks,
            SortOrder = 1,
        };
        db.Products.AddRange(friesProduct, softDrink);

        db.PaymentMethods.Add(new PaymentMethod
        {
            MethodName = "Telebirr",
            AccountName = "Terry Corner Burgers PLC",
            AccountNumberOrIdentifier = "0911000000",
            PhoneNumber = "0911000000",
            Instructions = "Send the exact order total to this Telebirr number, then upload your receipt.",
            IsActive = true,
            SortOrder = 1,
        });
        db.PaymentMethods.Add(new PaymentMethod
        {
            MethodName = "Bank Transfer — CBE",
            AccountName = "Terry Corner Burgers PLC",
            AccountNumberOrIdentifier = "1000123456789",
            Instructions = "Transfer the exact order total to this CBE account, then upload your receipt.",
            IsActive = true,
            SortOrder = 2,
        });

        var doubleCombo = new Promotion
        {
            Name = "Double Burger Combo",
            Description = "Terry's Double + Fries + Drink at a special price.",
            DiscountType = Domain.Enums.DiscountType.FixedAmount,
            DiscountValue = 301, // ETB 1,200 -> ETB 899 example from spec
            StartDateUtc = DateTime.UtcNow.AddDays(-1),
            EndDateUtc = DateTime.UtcNow.AddDays(30),
            IsFeatured = true,
            IsActive = true,
        };
        db.Promotions.Add(doubleCombo);
        db.PromotionProducts.Add(new PromotionProduct { Promotion = doubleCombo, Product = doubleBurger });

        await db.SaveChangesAsync();
    }
}
