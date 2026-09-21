using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TerryCorner.Application.Common.Interfaces;
using TerryCorner.Domain.Entities;
using TerryCorner.Domain.Enums;

namespace TerryCorner.Application.Features.Orders;

public record CreateOrderCommand(CreateOrderRequest Request) : IRequest<OrderDto>;

public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.Request.ContactFullName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Request.ContactPhoneNumber).NotEmpty().MaximumLength(30);
        RuleFor(x => x.Request.OrderType).NotEmpty().Must(t => t is "Pickup" or "Delivery")
            .WithMessage("OrderType must be 'Pickup' or 'Delivery'.");
        RuleFor(x => x.Request.DeliveryAddress)
            .NotEmpty()
            .When(x => x.Request.OrderType == "Delivery")
            .WithMessage("Delivery address is required for delivery orders.");
        RuleFor(x => x.Request.Items).NotEmpty().WithMessage("Your order must contain at least one item.");
        RuleForEach(x => x.Request.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.Quantity).GreaterThan(0).LessThanOrEqualTo(50);
        });
    }
}

public class OrderValidationException : Exception
{
    public OrderValidationException(string message) : base(message) { }
}

public class CreateOrderCommandHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUser) : IRequestHandler<CreateOrderCommand, OrderDto>
{
    public async Task<OrderDto> Handle(CreateOrderCommand command, CancellationToken ct)
    {
        var request = command.Request;

        var productIds = request.Items.Select(i => i.ProductId).Distinct().ToList();
        var products = await db.Products
            .Include(p => p.ProductToppings).ThenInclude(pt => pt.Topping)
            .Where(p => productIds.Contains(p.Id))
            .ToListAsync(ct);

        if (products.Count != productIds.Count)
        {
            throw new OrderValidationException("One or more items in your order are no longer available.");
        }

        var customer = await ResolveCustomerAsync(request, ct);

        var order = new Order
        {
            OrderNumber = await GenerateOrderNumberAsync(ct),
            Customer = customer,
            Status = OrderStatus.PaymentPending,
            PaymentStatus = PaymentStatus.Pending,
            OrderType = request.OrderType == "Delivery" ? OrderType.Delivery : OrderType.Pickup,
            ContactFullName = request.ContactFullName,
            ContactPhoneNumber = request.ContactPhoneNumber,
            DeliveryAddress = request.OrderType == "Delivery" ? request.DeliveryAddress : null,
            OrderNotes = request.OrderNotes,
        };

        decimal subtotal = 0;

        foreach (var itemRequest in request.Items)
        {
            // Server is authoritative: the product's CURRENT price/availability is looked up here,
            // never taken from the client. Same for toppings below.
            var product = products.Single(p => p.Id == itemRequest.ProductId);

            if (!product.IsAvailable)
            {
                throw new OrderValidationException($"'{product.Name}' is currently sold out.");
            }

            var availableToppingIds = product.ProductToppings.Select(pt => pt.ToppingId).ToHashSet();
            var invalidToppingIds = itemRequest.ToppingIds.Where(id => !availableToppingIds.Contains(id)).ToList();
            if (invalidToppingIds.Count > 0)
            {
                throw new OrderValidationException($"One or more selected toppings are not valid for '{product.Name}'.");
            }

            var selectedToppings = product.ProductToppings
                .Where(pt => itemRequest.ToppingIds.Contains(pt.ToppingId))
                .Select(pt => pt.Topping)
                .ToList();

            var unavailableTopping = selectedToppings.FirstOrDefault(t => !t.IsAvailable);
            if (unavailableTopping is not null)
            {
                throw new OrderValidationException($"'{unavailableTopping.Name}' is currently unavailable.");
            }

            var toppingsTotal = selectedToppings.Sum(t => t.AdditionalPrice);
            var unitPrice = product.Price + toppingsTotal;
            var lineTotal = unitPrice * itemRequest.Quantity;
            subtotal += lineTotal;

            var orderItem = new OrderItem
            {
                Product = product,
                ProductNameSnapshot = product.Name,
                UnitPriceSnapshot = unitPrice,
                Quantity = itemRequest.Quantity,
                LineTotal = lineTotal,
            };

            foreach (var topping in selectedToppings)
            {
                orderItem.Toppings.Add(new OrderItemTopping
                {
                    Topping = topping,
                    ToppingNameSnapshot = topping.Name,
                    AdditionalPriceSnapshot = topping.AdditionalPrice,
                });
            }

            order.Items.Add(orderItem);
        }

        // Promotions are applied automatically by matching active promotions to items in a later
        // pass (Phase 5 admin-configured promotions integration); no discount is applied yet.
        order.Subtotal = subtotal;
        order.DiscountAmount = 0;
        order.Total = subtotal;

        order.StatusHistory.Add(new OrderStatusHistory
        {
            Status = OrderStatus.OrderReceived,
            PaymentStatus = PaymentStatus.Pending,
            ChangedBy = currentUser.UserId ?? "guest",
        });
        order.StatusHistory.Add(new OrderStatusHistory
        {
            Status = OrderStatus.PaymentPending,
            PaymentStatus = PaymentStatus.Pending,
            ChangedBy = "system",
            Reason = "Order created; awaiting off-system payment.",
        });

        db.Orders.Add(order);
        await db.SaveChangesAsync(ct);

        return ToDto(order);
    }

    private async Task<Customer> ResolveCustomerAsync(CreateOrderRequest request, CancellationToken ct)
    {
        var userId = currentUser.UserId;

        if (userId is not null)
        {
            var existing = await db.Customers.FirstOrDefaultAsync(c => c.ApplicationUserId == userId, ct);
            if (existing is not null)
            {
                return existing;
            }
        }

        // Guest checkout: create a lightweight customer record with no linked account.
        return new Customer
        {
            ApplicationUserId = userId,
            FullName = request.ContactFullName,
            PhoneNumber = request.ContactPhoneNumber,
        };
    }

    private async Task<string> GenerateOrderNumberAsync(CancellationToken ct)
    {
        // Simple incrementing scheme (TC-1000, TC-1001, ...). Not strictly race-proof under very
        // high concurrent load; acceptable for this stage given the low write concurrency of a
        // single-restaurant ordering flow. Revisit with a DB sequence if that ever changes.
        var lastNumber = await db.Orders
            .OrderByDescending(o => o.CreatedAtUtc)
            .Select(o => o.OrderNumber)
            .FirstOrDefaultAsync(ct);

        var nextSequence = 1000;
        if (lastNumber is not null && lastNumber.StartsWith("TC-") && int.TryParse(lastNumber[3..], out var parsed))
        {
            nextSequence = parsed + 1;
        }

        return $"TC-{nextSequence}";
    }

    private static OrderDto ToDto(Order order) => new(
        order.Id,
        order.OrderNumber,
        order.Status.ToString(),
        order.PaymentStatus.ToString(),
        order.OrderType.ToString(),
        order.Subtotal,
        order.DiscountAmount,
        order.Total,
        order.Items.Select(i => new OrderItemDto(
            i.ProductId,
            i.ProductNameSnapshot,
            i.UnitPriceSnapshot,
            i.Quantity,
            i.Toppings.Select(t => t.ToppingNameSnapshot).ToList(),
            i.LineTotal)).ToList(),
        order.CreatedAtUtc);
}
