using JobWize.Frontend.Shared.Navigation;
using MudBlazor;

namespace JobWize.Frontend.Modules.Dashboard
{
    internal static class DependencyInjection
    {
        public static IServiceCollection AddDashboardModule(this IServiceCollection services)
        {
            services.AddSingleton<INavItem>(new NavItem(
                Label: "Dashboard",
                Href: DashboardRoutes.Home,
                Icon: Icons.Material.Filled.Dashboard,
                Order: 0));

            return services;
        }
    }
}
