using ECommerce.Api.Data;
using ECommerce.Api.Modules.Catalog.Contracts;
using ECommerce.Api.Modules.Inventory.Contracts;

namespace ECommerce.Api.Modules.Inventory;

public sealed class StockLedger(AppDbContext context, IProductCatalog catalog) : IStockLedger
{
    public async Task RestoreAsync(
        IReadOnlyCollection<StockRestoreLine> lines,
        string transactionType,
        string note,
        int? actorUserId,
        CancellationToken cancellationToken = default)
    {
        foreach (var line in lines)
        {
            var change = await catalog.AdjustStockAsync(line.ProductId, line.Quantity, cancellationToken);
            if (change is null)
            {
                continue;
            }

            context.InventoryTransactions.Add(new InventoryTransaction
            {
                ProductID = line.ProductId,
                TransactionType = transactionType,
                Quantity = line.Quantity,
                PreviousStock = change.PreviousStock,
                NewStock = change.NewStock,
                Note = note,
                CreatedBy = actorUserId,
                CreatedAt = DateTime.UtcNow
            });
        }
    }
}
