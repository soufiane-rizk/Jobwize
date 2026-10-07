using JobWize.Modules.Identity.Persistence;
using JobWize.Runtime.Contracts.Dispatching;
using JobWize.Shared.Application.Results;
using JobWize.Shared.Endpoints;
using JobWize.Shared.Runtime.Contracts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using static JobWize.Modules.Identity.Contracts.Public.Users.GetUsers;


namespace JobWize.Modules.Identity.Application.Users
{
    public static class GetUsers
    {
        public sealed record Query(bool IsSuperAdmin) : IQuery<Response>;

        internal sealed class Endpoint : IEndpoint
        {
            public void MapEndpoint(IEndpointRouteBuilder app)
            {
                app.MapGet(
                    Contracts.Public.Users.GetUsers.Route,
                    async (
                        HttpContext httpContext,
                        IDispatcher dispatcher,
                        CancellationToken cancellationToken) =>
                    {
                        var query = new Query(httpContext.User.IsInRole("SuperAdmin"));

                        var result = await dispatcher.SendAsync(query, cancellationToken);

                        return result.ToApiResult();
                    })
                    .RequireAuthorization(global::JobWize.Modules.Identity.Contracts.Public.Authentication.AuthenticationPolicies.UserManagement)
                    .WithName("GetUsers")
                    .WithTags("Users");
            }
        }

        internal sealed class Handler : IQueryHandler<Query, Response>
        {
            private readonly IdentityDbContext _dbContext;
            public Handler(IdentityDbContext dbContext)
            {
                _dbContext = dbContext;
            }
            public async Task<Result<Response>> HandleAsync(Query query, CancellationToken cancellationToken)
            {
                var userRows = await _dbContext.Users
                    .Where(u => query.IsSuperAdmin || u.Role == Domain.Enums.UserRole.Candidate)
                    .OrderBy(u => u.LastName).ThenBy(u => u.FirstName)
                    .Select(u => new { u.Id, u.FirstName, u.LastName, u.Email, u.Role, u.Status, u.MustChangePassword, u.CreatedAt })
                    .ToListAsync(cancellationToken);

                var users = userRows
                    .Select(u => new UserDto(
                        u.Id, u.FirstName, u.LastName, u.Email,
                        (Contracts.Public.Authentication.UserRole)u.Role,
                        (Contracts.Public.Users.UserStatus)u.Status,
                        u.MustChangePassword, u.CreatedAt))
                    .ToList();

                return Result<Response>.Success(new Response(users));
            }
        }
    }
}
