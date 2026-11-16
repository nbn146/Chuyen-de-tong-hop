using ECommerce.Api.Modules.Inventory.Contracts;

namespace ECommerce.Api.Modules.Inventory;

public static class InventoryModule
{
    public static IServiceCollection AddInventoryModule(this IServiceCollection services)
    {
        services.AddScoped<IInventoryService, InventoryService>();
        services.AddScoped<IStockLedger, StockLedger>();
        return services;
    }
}
