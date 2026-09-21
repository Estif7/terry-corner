using MediatR;
using Microsoft.EntityFrameworkCore;
using TerryCorner.Application.Common.Interfaces;
using TerryCorner.Domain.Enums;

namespace TerryCorner.Application.Features.Orders;

public record OrderStatusStepDto(string Status, bool IsComplete, bool IsActive);

public record OrderTrackingDto(
    string OrderNumber,
    string Status,
    string PaymentStatus,
    decimal Total,
    IReadOnlyList<OrderStatusStepDto> Timeline,
    DateTime CreatedAtUtc);

public record GetOrderByNumberQuery(string OrderNumber) : IRequest<OrderTrackingDto>;

public class GetOrderByNumberQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetOrderByNumberQuery, OrderTrackingDto>
{
    // Fixed display order for the customer-facing timeline. Cancelled is handled separately.
    private static readonly OrderStatus[] TimelineSteps =
    {
        OrderStatus.OrderReceived,
        OrderStatus.PaymentPending,
        OrderStatus.PaymentReceiptSubmitted,
        OrderStatus.PaymentVerification,
        OrderStatus.PreparingFood,
        OrderStatus.Ready,
        OrderStatus.Completed,
    };

    public async Task<OrderTrackingDto> Handle(GetOrderByNumberQuery request, CancellationToken ct)
    {
        var order = await db.Orders
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.OrderNumber == request.OrderNumber, ct)
            ?? throw new KeyNotFoundException($"Order {request.OrderNumber} was not found.");

        var currentIndex = Array.IndexOf(TimelineSteps, order.Status);

        var timeline = TimelineSteps.Select((step, index) => new OrderStatusStepDto(
            step.ToString(),
            IsComplete: index < currentIndex || (index == currentIndex && order.Status == OrderStatus.Completed),
            IsActive: index == currentIndex)).ToList();

        return new OrderTrackingDto(
            order.OrderNumber,
            order.Status.ToString(),
            order.PaymentStatus.ToString(),
            order.Total,
            timeline,
            order.CreatedAtUtc);
    }
}
