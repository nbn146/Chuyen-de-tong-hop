
namespace ECommerce.Api.Modules.Inventory;

public interface IInventoryService
{
    Task<List<InventoryDto>> GetInventoryListAsync(string? keyword, CancellationToken cancellationToken = default);
    Task<InventoryDto> UpdateStockAsync(int productId, int adminUserId, UpdateStockDto dto, CancellationToken cancellationToken = default);
    Task<List<InventoryTransactionDto>> GetTransactionsAsync(int productId, CancellationToken cancellationToken = default);
}
