
namespace ECommerce.Api.Modules.ShoppingCart;

public interface ICartService
{
    Task<CartDto> GetAsync(int userId, CancellationToken ct = default);
    Task<CartDto> AddAsync(int userId, AddCartItemDto dto, CancellationToken ct = default);
    Task<CartDto> UpdateAsync(int userId, int productId, UpdateCartItemDto dto, CancellationToken ct = default);
    Task RemoveAsync(int userId, int productId, CancellationToken ct = default);
    Task ClearAsync(int userId, CancellationToken ct = default);
}
