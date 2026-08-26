using JobWize.Modules.Identity.Infrastructure.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

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

            var jwtOptions = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
                ?? throw new InvalidOperationException("JWT configuration section is missing.");

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtOptions.SecretKey)),
                    ClockSkew = TimeSpan.Zero
                };
            });

            services.AddAuthorization(options =>
            {
                options.DefaultPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .RequireClaim(global::JobWize.Modules.Identity.Contracts.Public.Authentication.AuthenticationClaimTypes.MustChangePassword, "false")
                    .Build();

                options.AddPolicy(
                    global::JobWize.Modules.Identity.Contracts.Public.Authentication.AuthenticationPolicies.PasswordChange,
                    new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
                        .RequireAuthenticatedUser()
                        .Build());

                options.AddPolicy(global::JobWize.Modules.Identity.Contracts.Public.Authentication.AuthenticationPolicies.UserManagement,
                    new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder().RequireAuthenticatedUser().RequireClaim(global::JobWize.Modules.Identity.Contracts.Public.Authentication.AuthenticationClaimTypes.MustChangePassword, "false").RequireRole("Admin", "SuperAdmin").Build());

                options.AddPolicy(global::JobWize.Modules.Identity.Contracts.Public.Authentication.AuthenticationPolicies.SuperAdmin,
                    new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder().RequireAuthenticatedUser().RequireClaim(global::JobWize.Modules.Identity.Contracts.Public.Authentication.AuthenticationClaimTypes.MustChangePassword, "false").RequireRole("SuperAdmin").Build());
            });

            return services;
        }
    }
}
