using FluentValidation;
using JobWize.Modules.Identity.Contracts.Public.Authentication;
using JobWize.Modules.Identity.Infrastructure.Authentication;
using JobWize.Modules.Identity.Persistence;
using JobWize.Runtime.Contracts.Dispatching;
using JobWize.Shared.Application.Results;
using JobWize.Shared.Endpoints;
using JobWize.Shared.Runtime.Contracts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using static JobWize.Modules.Identity.Contracts.Public.Authentication.Refresh;

namespace JobWize.Modules.Identity.Application.Authentication
{
    public static class Refresh
    {
        internal sealed record Command(string RefreshToken) : ICommand<AuthenticationResponse>;

        internal sealed class Validator : AbstractValidator<Command>
        {
            public Validator() => RuleFor(x => x.RefreshToken).NotEmpty();
        }

        internal sealed class Endpoint : IEndpoint
        {
            public void MapEndpoint(IEndpointRouteBuilder app)
            {
                app.MapPost(
                    Contracts.Public.Authentication.Refresh.Route,
                    async (Request request, IDispatcher dispatcher, CancellationToken cancellationToken) =>
                    {
                        Result<AuthenticationResponse> result = await dispatcher.SendAsync(new Command(request.RefreshToken), cancellationToken);
                        return result.ToApiResult();
                    })
                    .WithName("RefreshAuthentication")
                    .WithTags("Authentication");
            }
        }

        internal sealed class Handler : ICommandHandler<Command, AuthenticationResponse>
        {
            private readonly IUserRepository _userRepository;
            private readonly IRefreshTokenHasher _refreshTokenHasher;
            private readonly IAuthenticationSessionService _authenticationSessionService;

            public Handler(IUserRepository userRepository, IRefreshTokenHasher refreshTokenHasher, IAuthenticationSessionService authenticationSessionService)
            {
                _userRepository = userRepository;
                _refreshTokenHasher = refreshTokenHasher;
                _authenticationSessionService = authenticationSessionService;
            }

            public async Task<Result<AuthenticationResponse>> HandleAsync(Command command, CancellationToken cancellationToken)
            {
                Domain.User? user = await _userRepository.GetByRefreshTokenHashAsync(
                    _refreshTokenHasher.Hash(command.RefreshToken),
                    cancellationToken);

                if (user is null)
                    return Result<AuthenticationResponse>.Failure(IdentityErrors.RefreshTokenInvalid);

                Result<AuthenticationSession> sessionResult = await _authenticationSessionService.RefreshAsync(user, command.RefreshToken, cancellationToken);

                if (sessionResult.IsFailure)
                    return Result<AuthenticationResponse>.Failure(sessionResult.Error!);

                AuthenticationSession session = sessionResult.Value!;

                return Result<AuthenticationResponse>.Success(new AuthenticationResponse(
                    session.UserId,
                    session.FirstName,
                    session.LastName,
                    (UserRole)session.Role,
                    session.AccessToken,
                    session.RefreshToken,
                    session.AccessTokenExpiresAt,
                    session.RefreshTokenExpiresAt));
            }
        }
    }
}
