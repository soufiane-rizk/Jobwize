namespace JobWize.Api
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApi(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(options =>
            {
                options.CustomSchemaIds(type =>
                {
                    if (!type.IsNested)
                    {
                        return type.Name;
                    }

                    return $"{type.DeclaringType!.Name}.{type.Name}";
                });
            });

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
