namespace ECommerce.Api.Modules.Catalog;

public record ProductDto(
    int ProductID,
    int CategoryID,
    string CategoryName,
    string ProductName,
    string SKU,
    string? Description,
    decimal Price,
    int BrandID,
    string? BrandName,
    string? ImageUrl,
    int StockQuantity,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
