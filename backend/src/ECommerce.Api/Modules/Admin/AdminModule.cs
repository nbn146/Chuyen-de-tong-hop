namespace ECommerce.Api.Modules.Admin;

public static class AdminModule
{
    public static IServiceCollection AddAdminModule(this IServiceCollection services)
    {
        services.AddScoped<IAdminService, AdminService>();
        services.AddScoped<IReportService, ReportService>();
        return services;
    }
}
