using ECommerce.Api.Modules.Catalog.Contracts;

namespace ECommerce.Api.Modules.Catalog;

public static class CatalogModule
{
    public static IServiceCollection AddCatalogModule(this IServiceCollection services)
    {
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IProductCatalog, ProductCatalog>();
        return services;
    }
}
