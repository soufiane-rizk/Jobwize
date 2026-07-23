namespace JobWize.Api
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApi(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();

            services
                .AddHealthChecks()
                .AddNpgSql(
                    configuration.GetConnectionString("Identity")!,
                    name: "identity-postgres",
                    tags: ["ready"]);

            return services;
        }
    }
}
