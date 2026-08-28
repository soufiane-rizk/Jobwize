using JobWize.Frontend.Shared.Navigation;

namespace JobWize.Frontend.Modules.Companies;

internal static class DependencyInjection
{
    public static IServiceCollection AddCompaniesModule(this IServiceCollection services)
    {
        services.AddScoped<CompanyModerationService>();
        services.AddSingleton<INavItem>(new NavItem("Company reviews", CompaniesRoutes.ReviewQueue, MudBlazor.Icons.Material.Filled.Business, 11, "Admin,SuperAdmin"));
        return services;
    }
}
