using ECommerce.Api.Data;
using ECommerce.Api.Modules.Catalog.Contracts;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Api.Modules.Catalog;

public sealed class ProductCatalog(AppDbContext context) : IProductCatalog
{
    public Task<ProductSnapshot?> FindActiveAsync(int productId, CancellationToken cancellationToken = default) =>
        context.Products
            .AsNoTracking()
            .Where(p => p.ProductID == productId && p.IsActive)
            .Select(p => new ProductSnapshot(p.ProductID, p.ProductName, p.SKU, p.Price, p.StockQuantity))
            .SingleOrDefaultAsync(cancellationToken);

    public async Task<IReadOnlyList<ProductStockInfo>> ListStockAsync(
        string? keyword, CancellationToken cancellationToken = default)
    {
        var query = context.Products.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var kw = keyword.ToLower();
            query = query.Where(p => p.ProductName.ToLower().Contains(kw) || p.SKU.ToLower().Contains(kw));
        }

        return await query
            .OrderBy(p => p.ProductName)
            .Select(p => new ProductStockInfo(
                p.ProductID, p.ProductName, p.SKU, p.StockQuantity,
                p.Category.CategoryName, p.Brand.BrandName, p.IsActive))
            .ToListAsync(cancellationToken);
    }

    public Task<ProductStockInfo?> GetStockAsync(int productId, CancellationToken cancellationToken = default) =>
        context.Products
            .AsNoTracking()
            .Where(p => p.ProductID == productId)
            .Select(p => new ProductStockInfo(
                p.ProductID, p.ProductName, p.SKU, p.StockQuantity,
                p.Category.CategoryName, p.Brand.BrandName, p.IsActive))
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<StockChange?> AdjustStockAsync(int productId, int delta, CancellationToken cancellationToken = default)
    {
        var product = await context.Products
            .SingleOrDefaultAsync(p => p.ProductID == productId, cancellationToken);
        if (product is null)
        {
            return null;
        }

        var previous = product.StockQuantity;
        product.StockQuantity = previous + delta;
        product.UpdatedAt = DateTime.UtcNow;
        return new StockChange(previous, product.StockQuantity);
    }

    public async Task<IReadOnlyList<int>> GetActiveProductIdsAsync(CancellationToken cancellationToken = default) =>
        await context.Products
            .Where(p => p.IsActive)
            .OrderBy(p => p.ProductID)
            .Select(p => p.ProductID)
            .ToListAsync(cancellationToken);

    public async Task<ProductIndexSource?> GetIndexSourceAsync(int productId, CancellationToken cancellationToken = default)
    {
        var product = await context.Products
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.ProductID == productId, cancellationToken);

        return product is null ? null : ProductIndexSource.From(product);
    }
}
