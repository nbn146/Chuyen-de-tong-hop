using ECommerce.Api.Modules.ShoppingCart.Contracts;

namespace ECommerce.Api.Modules.ShoppingCart;

public static class ShoppingCartModule
{
    public static IServiceCollection AddShoppingCartModule(this IServiceCollection services)
    {
        services.AddScoped<ICartService, CartService>();
        services.AddScoped<ICheckoutCart, CheckoutCart>();
        return services;
    }
}
