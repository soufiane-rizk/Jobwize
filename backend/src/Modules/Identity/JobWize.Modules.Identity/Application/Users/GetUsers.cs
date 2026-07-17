using JobWize.Modules.Identity.Persistence;
using JobWize.Runtime.Contracts.Dispatching;
using JobWize.Shared.Application.Results;
using JobWize.Shared.Contracts.Application.Dispatching;
using JobWize.Shared.Endpoints;
using JobWize.Shared.Runtime.Handlers;
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
        public sealed record Query : IQuery<Result<Response>>;

        internal sealed class Endpoint : IEndpoint
        {
            public void MapEndpoint(IEndpointRouteBuilder app)
            {
                app.MapGet(
                    "/api/users",
                    async (
                        //[FromQuery] Request request,
                        IDispatcher dispatcher,
                        CancellationToken cancellationToken) =>
                    {
                        var query = new Query();

                        var result = await dispatcher.SendAsync(query, cancellationToken);

                        return result.ToApiResult();
                    })
                    .WithName("GetUsers")
                    .WithTags("Users");
            }
        }

        internal sealed class Handler : IQueryHandler<Query, Result<Response>>
        {
            private readonly IdentityDbContext _dbContext;
            public Handler(IdentityDbContext dbContext)
            {
                _dbContext = dbContext;
            }
            public async Task<Result<Response>> HandleAsync(Query query, CancellationToken cancellationToken)
            {
                var users = await _dbContext.Users
                    .Select(u => new UserDto(
                        u.Id,
                        u.FirstName,
                        u.LastName,
                        u.Email,
                        u.Role.ToString()))
                    .ToListAsync(cancellationToken);

                return Result<Response>.Success(new Response(users));
            }
        }
    }
}
