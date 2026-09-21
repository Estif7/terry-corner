namespace TerryCorner.Application.Features.Orders;

public record CreateOrderItemRequest(Guid ProductId, int Quantity, IReadOnlyList<Guid> ToppingIds);

public record CreateOrderRequest(
    string ContactFullName,
    string ContactPhoneNumber,
    string OrderType, // "Pickup" or "Delivery"
    string? DeliveryAddress,
    string? OrderNotes,
    IReadOnlyList<CreateOrderItemRequest> Items);

public record OrderItemDto(
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    int Quantity,
    IReadOnlyList<string> ToppingNames,
    decimal LineTotal);

public record OrderDto(
    Guid Id,
    string OrderNumber,
    string Status,
    string PaymentStatus,
    string OrderType,
    decimal Subtotal,
    decimal DiscountAmount,
    decimal Total,
    IReadOnlyList<OrderItemDto> Items,
    DateTime CreatedAtUtc);
