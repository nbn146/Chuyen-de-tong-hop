namespace ECommerce.Api.Modules.ShoppingCart;

public sealed record CartDto(
    IReadOnlyList<CartItemDto> Items,
    int TotalItems,
    decimal TotalAmount);
