using JobWize.Modules.Identity.Persistence;
using JobWize.Runtime.Contracts.Dispatching;
using JobWize.Shared.Application.Results;
using JobWize.Shared.Application.Security;
using JobWize.Shared.Endpoints;
using JobWize.Shared.Runtime.Contracts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using static JobWize.Modules.Identity.Contracts.Public.Users.GetCurrentUser;

namespace JobWize.Modules.Identity.Application.Users
{
    public static class GetCurrentUser
    {
        internal sealed record Query : IQuery<Response>;

        internal sealed class Endpoint : IEndpoint
        {
            public void MapEndpoint(IEndpointRouteBuilder app)
            {
                app.MapGet(
                    Contracts.Public.Users.GetCurrentUser.Route,
                    async (IDispatcher dispatcher, CancellationToken cancellationToken) =>
                    {
                        Result<Response> result = await dispatcher.SendAsync(new Query(), cancellationToken);

                        return result.ToApiResult();
                    })
                    .RequireAuthorization()
                    .WithName("GetCurrentUser")
                    .WithTags("Users");
            }
        }

        internal sealed class Handler : IQueryHandler<Query, Response>
        {
            private readonly IUserRepository _userRepository;
            private readonly IUserContext _userContext;

            public Handler(IUserRepository userRepository, IUserContext userContext)
            {
                _userRepository = userRepository;
                _userContext = userContext;
            }

            public async Task<Result<Response>> HandleAsync(Query query, CancellationToken cancellationToken)
            {
                Domain.User? user = await _userRepository.GetByIdAsync(_userContext.UserId, cancellationToken);

                if (user is null)
                {
                    return Result<Response>.Failure(IdentityErrors.UserNotFound);
                }

                return Result<Response>.Success(new Response(
                    user.Id,
                    user.FirstName,
                    user.LastName,
                    user.Email,
                    (Contracts.Public.Authentication.UserRole)user.Role,
                    user.AvatarUrl,
                    user.MustChangePassword));
            }
        }
    }
}
