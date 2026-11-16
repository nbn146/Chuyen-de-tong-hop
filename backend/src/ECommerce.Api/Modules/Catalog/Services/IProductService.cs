using ECommerce.Api.Shared;


namespace ECommerce.Api.Modules.Catalog;

public interface IProductService
{
    Task<PagedResult<ProductDto>> SearchProductsAsync(ProductSearchRequestDto request, bool includeInactive, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetActiveBrandsAsync(CancellationToken cancellationToken = default);
    Task<ProductDto> GetProductByIdAsync(int id, bool includeInactive, CancellationToken cancellationToken = default);
    Task<ProductDto> CreateProductAsync(ProductCreateDto dto, CancellationToken cancellationToken = default);
    Task<ProductDto> UpdateProductAsync(int id, ProductUpdateDto dto, CancellationToken cancellationToken = default);
    Task DeleteProductAsync(int id, CancellationToken cancellationToken = default);
}
