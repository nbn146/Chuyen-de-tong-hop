using ECommerce.Api.Data;
using ECommerce.Api.Modules.Inventory.Contracts;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Api.Modules.Inventory;

public sealed class StockLedger(AppDbContext context) : IStockLedger
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
            var product = await context.Products
                .SingleOrDefaultAsync(p => p.ProductID == line.ProductId, cancellationToken);
            if (product is null)
            {
                continue;
            }

            var previousStock = product.StockQuantity;
            product.StockQuantity += line.Quantity;
            product.UpdatedAt = DateTime.UtcNow;

            context.InventoryTransactions.Add(new InventoryTransaction
            {
                ProductID = line.ProductId,
                TransactionType = transactionType,
                Quantity = line.Quantity,
                PreviousStock = previousStock,
                NewStock = product.StockQuantity,
                Note = note,
                CreatedBy = actorUserId,
                CreatedAt = DateTime.UtcNow
            });
        }
    }
}
