using JobWize.Frontend.Shared.Navigation;
using MudBlazor;
namespace JobWize.Frontend.Modules.Applications;
internal static class DependencyInjection
{
    public static IServiceCollection AddApplicationsModule(this IServiceCollection services)
    {
        services.AddScoped<JobApplicationService>();
        services.AddSingleton<INavItem>(new NavItem("Applications", ApplicationsRoutes.List, Icons.Material.Filled.WorkOutline, 10));
        return services;
    }
}
