using Microsoft.AspNetCore.SignalR;

namespace TerryCorner.Api.Hubs;

public class OrderTrackingHub : Hub
{
    private static string GroupName(string orderNumber) => $"order-{orderNumber}";

    /// <summary>Called by the client's tracking page so it only receives updates for the order it's viewing.</summary>
    public async Task JoinOrderGroup(string orderNumber)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(orderNumber));
    }

    public async Task LeaveOrderGroup(string orderNumber)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName(orderNumber));
    }
}
