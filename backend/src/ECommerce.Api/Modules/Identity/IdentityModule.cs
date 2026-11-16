using ECommerce.Api.Modules.Identity.Contracts;

namespace ECommerce.Api.Modules.Identity;

public static class IdentityModule
{
    public static IServiceCollection AddIdentityModule(this IServiceCollection services, IHostEnvironment environment)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IProfileService, ProfileService>();
        services.AddScoped<IAddressService, AddressService>();
        services.AddScoped<IAddressLookup, AddressLookup>();
        services.AddScoped<IUserDirectory, UserDirectory>();
        services.AddSingleton(serviceProvider =>
        {
            var section = serviceProvider
                .GetRequiredService<IConfiguration>()
                .GetSection("BootstrapAdmin");

            return new BootstrapAdminOptions(
                section["Email"],
                section["Password"],
                section["FullName"]);
        });
        services.AddScoped<DevelopmentAdminBootstrapper>();

        if (environment.IsDevelopment())
        {
            services.AddHostedService<DevelopmentAdminBootstrapHostedService>();
        }

        return services;
    }
}
