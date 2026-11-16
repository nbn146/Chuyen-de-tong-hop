using Microsoft.AspNetCore.Authorization;
using ECommerce.Api.Shared.Extensions;
using Microsoft.AspNetCore.Mvc;
using ECommerce.Api.Modules.Identity;

namespace ECommerce.Api.Modules.Inventory;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin")]
public class InventoryController : ControllerBase
{
    private readonly IInventoryService _inventoryService;

    public InventoryController(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    // US-4: Get inventory list
    [HttpGet]
    public async Task<IActionResult> GetInventory([FromQuery] string? keyword, CancellationToken cancellationToken)
    {
        var result = await _inventoryService.GetInventoryListAsync(keyword, cancellationToken);
        return Ok(result);
    }

    // US-4: Update stock
    [HttpPut("{productId}")]
    public async Task<IActionResult> UpdateStock(int productId, UpdateStockDto dto, CancellationToken cancellationToken)
    {
        var adminUserId = User.GetRequiredUserId();
        var result = await _inventoryService.UpdateStockAsync(productId, adminUserId, dto, cancellationToken);
        return Ok(result);
    }

    // US-4: Get transaction history
    [HttpGet("{productId}/transactions")]
    public async Task<IActionResult> GetTransactions(int productId, CancellationToken cancellationToken)
    {
        var result = await _inventoryService.GetTransactionsAsync(productId, cancellationToken);
        return Ok(result);
    }
}
