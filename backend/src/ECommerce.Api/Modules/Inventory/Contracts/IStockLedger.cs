namespace ECommerce.Api.Modules.Inventory.Contracts;

/// <summary>A quantity of one product to put back into stock.</summary>
public sealed record StockRestoreLine(int ProductId, int Quantity);

/// <summary>
/// Public contract of the Inventory module for changing stock levels. Other modules must use this
/// instead of touching products or inventory transactions directly. Changes are tracked on the
/// shared unit of work; the caller decides when to save.
/// </summary>
public interface IStockLedger
{
    Task RestoreAsync(
        IReadOnlyCollection<StockRestoreLine> lines,
        string transactionType,
        string note,
        int? actorUserId,
        CancellationToken cancellationToken = default);
}
