namespace ECommerce.Api.Modules.ShoppingCart.Contracts;

public sealed record CheckoutLine(
    int ProductId,
    string ProductName,
    string Sku,
    decimal UnitPrice,
    int Quantity,
    int StockQuantity,
    bool IsActive);

/// <summary>Public contract of the ShoppingCart module used by Orders at checkout.</summary>
public interface ICheckoutCart
{
    /// <summary>Returns the cart lines in insertion order, or null when the user has no cart.</summary>
    Task<IReadOnlyList<CheckoutLine>?> GetLinesAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>Marks the user's cart lines for removal; the caller saves the unit of work.</summary>
    Task ClearAsync(int userId, CancellationToken cancellationToken = default);
}
