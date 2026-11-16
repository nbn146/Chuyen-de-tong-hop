
namespace ECommerce.Api.Modules.Catalog;

public interface ICategoryService
{
    Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync(CancellationToken cancellationToken = default);
    Task<CategoryDto> GetCategoryByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<CategoryDto> CreateCategoryAsync(CategoryCreateDto dto, CancellationToken cancellationToken = default);
    Task<CategoryDto> UpdateCategoryAsync(int id, CategoryUpdateDto dto, CancellationToken cancellationToken = default);
    Task DeleteCategoryAsync(int id, CancellationToken cancellationToken = default);
}
