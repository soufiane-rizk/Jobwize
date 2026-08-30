using JobWize.Frontend.Shared.Navigation;

namespace JobWize.Frontend.Modules.Companies;

internal static class DependencyInjection
{
    public static IServiceCollection AddCompaniesModule(this IServiceCollection services)
    {
        services.AddScoped<CandidateCompanyService>();
        services.AddScoped<CompanyModerationService>();
        services.AddSingleton<INavItem>(new NavItem(
            "Company reviews",
            CompaniesRoutes.ReviewQueue,
            MudBlazor.Icons.Material.Filled.RateReview,
            11,
            "Admin,SuperAdmin"));
        services.AddSingleton<INavItem>(new NavItem(
            "Contact reviews",
            CompaniesRoutes.ContactReviewQueue,
            MudBlazor.Icons.Material.Filled.Contacts,
            12,
            "Admin,SuperAdmin"));
        services.AddSingleton<INavItem>(new NavItem(
            "Company catalogue",
            CompaniesRoutes.CatalogueManagement,
            MudBlazor.Icons.Material.Filled.Business,
            13,
            "Admin,SuperAdmin"));
        return services;
    }
}
