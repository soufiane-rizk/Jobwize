namespace JobWize.Frontend.Modules.Identity
{
    internal static class DependencyInjection
    {
        public static IServiceCollection AddIdentityModule(this IServiceCollection services)
        {
            services.AddScoped<Authentication.AuthenticationService>();
            services.AddScoped<Users.CurrentUserService>();
            services.AddScoped<Users.UserManagementService>();
            services.AddSingleton<Shared.Navigation.INavItem>(new Shared.Navigation.NavItem(
                Label: "Users",
                Href: IdentityRoutes.Users,
                Icon: MudBlazor.Icons.Material.Filled.People,
                Order: 10,
                Roles: "Admin,SuperAdmin"));

            return services;
        }
    }
}
