namespace JobWize.Frontend.Modules.Identity
{
    internal static class DependencyInjection
    {
        public static IServiceCollection AddIdentityModule(this IServiceCollection services)
        {
            services.AddScoped<Authentication.AuthenticationService>();

            return services;
        }
    }
}
