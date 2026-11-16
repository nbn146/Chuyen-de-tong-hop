namespace ECommerce.Api.Modules.ShoppingCart;

public sealed record CartItemDto(
    int ProductID,
    string ProductName,
    string SKU,
    decimal UnitPrice,
    int Quantity,
    int StockQuantity,
    string? ImageURL,
    decimal LineTotal);
