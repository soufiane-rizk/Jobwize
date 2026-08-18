using FluentValidation;
using JobWize.Modules.Identity.Contracts.Events.Authentication;
using JobWize.Modules.Identity.Contracts.Public.Authentication;
using JobWize.Modules.Identity.Infrastructure.Authentication;
using JobWize.Modules.Identity.Persistence;
using JobWize.Runtime.Contracts.Dispatching;
using JobWize.Shared.Application.Results;
using JobWize.Shared.Endpoints;
using JobWize.Shared.Infrastructure.Time;
using JobWize.Shared.Runtime.Contracts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using static JobWize.Modules.Identity.Contracts.Public.Authentication.Login;

namespace JobWize.Modules.Identity.Application.Authentication
{
    public static class Login
    {
        internal sealed record Command(string Username, string Password) : ICommand<AuthenticationResponse>;

        internal sealed class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(x => x.Username)
                    .NotEmpty();

                RuleFor(x => x.Password)
                    .NotEmpty();
            }
        }

        internal sealed class Endpoint : IEndpoint
        {
            public void MapEndpoint(IEndpointRouteBuilder app)
            {
                app.MapPost(
                    Contracts.Public.Authentication.Login.Route,
                    async (Request request, IDispatcher dispatcher, CancellationToken cancellationToken) =>
                    {
                        var command = new Command(request.Username, request.Password);
                        var result = await dispatcher.SendAsync(command, cancellationToken);

                        return result.ToApiResult();
                    })
                    .WithName("Login")
                    .WithTags("Authentication");
            }
        }

        internal sealed class Handler : ICommandHandler<Command, AuthenticationResponse>
        {
            private readonly IUserRepository _userRepository;
            private readonly IPasswordHasher _passwordHasher;
            private readonly IAuthenticationSessionService _authenticationSessionService;
            private readonly IDispatcher _dispatcher;

            public Handler(IUserRepository userRepository, IPasswordHasher passwordHasher, IAuthenticationSessionService authenticationService, IDispatcher dispatcher)
            {
                _userRepository = userRepository;
                _passwordHasher = passwordHasher;
                _authenticationSessionService = authenticationService;
                _dispatcher = dispatcher;
            }

            public async Task<Result<AuthenticationResponse>> HandleAsync(Command command, CancellationToken cancellationToken)
            {
                var user = await _userRepository.GetByEmailAsync(command.Username, cancellationToken);

                if (user is null)
                    return Result<AuthenticationResponse>.Failure(IdentityErrors.InvalidCredentials);

                if (!_passwordHasher.Verify(command.Password, user.PasswordHash))
                    return Result<AuthenticationResponse>.Failure(IdentityErrors.InvalidCredentials);

                    var session = await _authenticationSessionService.AuthenticateAsync(
                    user,
                    cancellationToken);

                await _dispatcher.PublishAsync(new UserLoggedIn(user.Id), cancellationToken);

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
