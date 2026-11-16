using ECommerce.Api.Modules.Orders.Contracts;

namespace ECommerce.Api.Modules.Orders;

public static class OrdersModule
{
    public static IServiceCollection AddOrdersModule(this IServiceCollection services)
    {
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IOrderAdministration, OrderAdministration>();
        return services;
    }
}
