using ECommerce.Api.Modules.Catalog.Contracts;
using ECommerce.Api.Data;
using Microsoft.EntityFrameworkCore;
using ECommerce.Api.Modules.Catalog;
using ECommerce.Api.Shared.Exceptions;

namespace ECommerce.Api.Modules.Inventory;

public class InventoryService : IInventoryService
{
    private readonly AppDbContext _context;

    private static readonly string[] ValidTransactionTypes = ["Import", "Export", "Adjustment"];

    private readonly IProductCatalog _catalog;

    public InventoryService(AppDbContext context, IProductCatalog catalog)
    {
        _context = context;
        _catalog = catalog;
    }

    public async Task<List<InventoryDto>> GetInventoryListAsync(string? keyword, CancellationToken cancellationToken = default)
    {
        var items = await _catalog.ListStockAsync(keyword, cancellationToken);
        return items.Select(ToDto).ToList();
    }

    public async Task<InventoryDto> UpdateStockAsync(int productId, int adminUserId, UpdateStockDto dto, CancellationToken cancellationToken = default)
    {
        if (!ValidTransactionTypes.Contains(dto.TransactionType))
            throw new DomainValidationException();

        var product = await _catalog.GetStockAsync(productId, cancellationToken)
            ?? throw new ResourceNotFoundException();

        var previousStock = product.StockQuantity;
        int newStock;

        switch (dto.TransactionType)
        {
            case "Import":
                if (dto.Quantity <= 0)
                    throw new DomainValidationException();
                try { newStock = checked(previousStock + dto.Quantity); }
                catch (OverflowException) { throw new DomainValidationException(); }
                break;
            case "Export":
                if (dto.Quantity <= 0)
                    throw new DomainValidationException();
                if (dto.Quantity > previousStock)
                    throw new DomainConflictException();
                newStock = previousStock - dto.Quantity;
                break;
            case "Adjustment":
                newStock = dto.Quantity; // Set absolute value
                if (newStock < 0)
                    throw new DomainValidationException();
                break;
            default:
                throw new DomainValidationException();
        }

        await _catalog.AdjustStockAsync(productId, newStock - previousStock, cancellationToken);

        var storedType = dto.TransactionType switch
        {
            "Import" => "IMPORT",
            "Export" => "SALE",
            _ => "ADJUSTMENT"
        };

        _context.InventoryTransactions.Add(new InventoryTransaction
        {
            ProductID = productId,
            TransactionType = storedType,
            Quantity = newStock - previousStock,
            PreviousStock = previousStock,
            NewStock = newStock,
            Note = dto.Note,
            CreatedBy = adminUserId,
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync(cancellationToken);

        return ToDto(product with { StockQuantity = newStock });
    }

    private static InventoryDto ToDto(ProductStockInfo p) => new()
    {
        ProductId = p.ProductId,
        ProductName = p.ProductName,
        Sku = p.Sku,
        StockQuantity = p.StockQuantity,
        CategoryName = p.CategoryName,
        BrandName = p.BrandName,
        IsActive = p.IsActive
    };

    public async Task<List<InventoryTransactionDto>> GetTransactionsAsync(int productId, CancellationToken cancellationToken = default)
    {
        return await _context.InventoryTransactions
            .Where(t => t.ProductID == productId)
            .Include(t => t.CreatedByUser)
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new InventoryTransactionDto
            {
                TransactionId = t.InventoryTransactionID,
                ProductId = t.ProductID,
                ProductName = t.Product.ProductName,
                TransactionType = t.TransactionType,
                Quantity = t.Quantity,
                PreviousStock = t.PreviousStock,
                NewStock = t.NewStock,
                Note = t.Note,
                CreatedByName = t.CreatedByUser != null ? t.CreatedByUser.FullName : null,
                CreatedAt = t.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }
}
