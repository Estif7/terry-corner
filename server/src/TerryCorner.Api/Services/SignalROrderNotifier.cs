using Microsoft.AspNetCore.SignalR;
using TerryCorner.Api.Hubs;
using TerryCorner.Application.Common.Interfaces;

namespace TerryCorner.Api.Services;

public class SignalROrderNotifier(IHubContext<OrderTrackingHub> hubContext) : IOrderNotifier
{
    public async Task NotifyOrderStatusChangedAsync(string orderNumber, CancellationToken ct)
    {
        // Payload is deliberately minimal — the client re-fetches GET /api/orders/{orderNumber}
        // as the source of truth rather than trusting a push payload to be fully up to date.
        await hubContext.Clients.Group($"order-{orderNumber}").SendAsync(
            "OrderUpdated", new { orderNumber }, ct);
    }
}
