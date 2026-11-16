using ECommerce.Api.Modules.Catalog;

namespace ECommerce.Api.Modules.Catalog.Contracts;

/// <summary>An active product as seen by shopping flows.</summary>
public sealed record ProductSnapshot(
    int ProductId,
    string ProductName,
    string Sku,
    decimal Price,
    int StockQuantity);

/// <summary>Product identity and stock level with display names, for inventory screens.</summary>
public sealed record ProductStockInfo(
    int ProductId,
    string ProductName,
    string Sku,
    int StockQuantity,
    string CategoryName,
    string BrandName,
    bool IsActive);

public sealed record StockChange(int PreviousStock, int NewStock);

/// <summary>The product facts used to build the AI knowledge document.</summary>
public sealed record ProductIndexSource(
    int ProductId,
    string ProductName,
    string Sku,
    string BrandName,
    string CategoryName,
    string? Description,
    string? Specifications,
    bool IsActive)
{
    public static ProductIndexSource From(Product product) => new(
        product.ProductID,
        product.ProductName,
        product.SKU,
        product.Brand.BrandName,
        product.Category.CategoryName,
        product.Description,
        product.Specifications,
        product.IsActive);
}

/// <summary>
/// Public contract of the Catalog module. Other modules read product data and change stock levels
/// only through this interface. Writes are tracked on the shared unit of work; callers save.
/// </summary>
public interface IProductCatalog
{
    /// <summary>Returns the product when it exists and is active, otherwise null.</summary>
    Task<ProductSnapshot?> FindActiveAsync(int productId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ProductStockInfo>> ListStockAsync(string? keyword, CancellationToken cancellationToken = default);

    Task<ProductStockInfo?> GetStockAsync(int productId, CancellationToken cancellationToken = default);

    /// <summary>Adds <paramref name="delta"/> to the stock level. Returns null when the product does not exist.</summary>
    Task<StockChange?> AdjustStockAsync(int productId, int delta, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<int>> GetActiveProductIdsAsync(CancellationToken cancellationToken = default);

    Task<ProductIndexSource?> GetIndexSourceAsync(int productId, CancellationToken cancellationToken = default);
}
