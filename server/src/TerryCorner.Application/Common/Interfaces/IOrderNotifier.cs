namespace TerryCorner.Application.Common.Interfaces;

/// <summary>
/// Notifies interested clients (customer tracking page) that an order's status changed.
/// Implemented in the API layer using SignalR, so Application never references it directly.
/// </summary>
public interface IOrderNotifier
{
    Task NotifyOrderStatusChangedAsync(string orderNumber, CancellationToken ct);
}
