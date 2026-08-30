using JobWize.Frontend.Shared.Navigation;
using MudBlazor;

namespace JobWize.Frontend.Modules.Files;

internal static class DependencyInjection
{
    public static IServiceCollection AddFilesModule(this IServiceCollection services)
    {
        services.AddScoped<CandidateDocumentService>();
        services.AddSingleton<INavItem>(new NavItem(
            "Documents",
            FilesRoutes.List,
            Icons.Material.Filled.Description,
            20));
        return services;
    }
}
