using FluentValidation;
using JobWize.Modules.Identity.Contracts.Events.Authentication;
using JobWize.Modules.Identity.Contracts.Public.Authentication;
using JobWize.Modules.Identity.Infrastructure.Authentication;
using JobWize.Modules.Identity.Persistence;
using JobWize.Runtime.Contracts.Dispatching;
using JobWize.Shared.Application.Results;
using JobWize.Shared.Application.Security;
using JobWize.Shared.Endpoints;
using JobWize.Shared.Infrastructure.Time;
using JobWize.Shared.Runtime.Contracts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using static JobWize.Modules.Identity.Contracts.Public.Authentication.ChangePassword;

namespace JobWize.Modules.Identity.Application.Authentication;

public static class ChangePassword
{
    internal sealed record Command(string CurrentPassword, string NewPassword, string ConfirmPassword) : ICommand<AuthenticationResponse>;

    internal sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(command => command.CurrentPassword).NotEmpty();
            RuleFor(command => command.NewPassword).NotEmpty().MinimumLength(8).MaximumLength(100);
            RuleFor(command => command.ConfirmPassword)
                .Equal(command => command.NewPassword)
                .WithMessage("The confirmation password does not match the new password.");
        }
    }

    internal sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost(
                    Contracts.Public.Authentication.ChangePassword.Route,
                    async (Request request, IDispatcher dispatcher, CancellationToken cancellationToken) =>
                    {
                        Result<AuthenticationResponse> result = await dispatcher.SendAsync(
                            new Command(request.CurrentPassword, request.NewPassword, request.ConfirmPassword),
                            cancellationToken);

                        return result.ToApiResult();
                    })
                .RequireAuthorization(AuthenticationPolicies.PasswordChange)
                .WithName("ChangePassword")
                .WithTags("Authentication");
        }
    }

    internal sealed class Handler(
        IUserRepository userRepository,
        IUserContext userContext,
        IPasswordHasher passwordHasher,
        IAuthenticationSessionService authenticationSessionService,
        IClock clock,
        IDispatcher dispatcher) : ICommandHandler<Command, AuthenticationResponse>
    {
        public async Task<Result<AuthenticationResponse>> HandleAsync(Command command, CancellationToken cancellationToken)
        {
            Domain.User? user = await userRepository.GetByIdAsync(userContext.UserId, cancellationToken);
            if (user is null)
            {
                return Result<AuthenticationResponse>.Failure(IdentityErrors.UserNotFound);
            }

            if (!passwordHasher.Verify(command.CurrentPassword, user.PasswordHash))
            {
                return Result<AuthenticationResponse>.Failure(IdentityErrors.InvalidCurrentPassword);
            }

            user.ChangePassword(passwordHasher.Hash(command.NewPassword));
            user.RevokeAllRefreshTokens(clock.UtcNow);

            AuthenticationSession session = await authenticationSessionService.AuthenticateAsync(user, cancellationToken);

            await dispatcher.PublishAsync(new PasswordChanged(user.Id), cancellationToken);

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
