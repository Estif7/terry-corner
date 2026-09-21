namespace TerryCorner.Domain.Enums;

public enum OrderStatus
{
    OrderReceived = 0,
    PaymentPending = 1,
    PaymentReceiptSubmitted = 2,
    PaymentVerification = 3,
    PreparingFood = 4,
    Ready = 5,
    Completed = 6,
    Cancelled = 7,
}

public enum PaymentStatus
{
    Pending = 0,
    ReceiptSubmitted = 1,
    Verified = 2,
    Rejected = 3,
}

public enum OrderType
{
    Pickup = 0,
    Delivery = 1,
}

public enum DiscountType
{
    Percentage = 0,
    FixedAmount = 1,
}
