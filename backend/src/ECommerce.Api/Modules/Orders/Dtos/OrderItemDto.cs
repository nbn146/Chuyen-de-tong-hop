namespace ECommerce.Api.Modules.Orders;

public sealed record OrderItemDto(
    int ProductID,
    string ProductName,
    string SKU,
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice);
