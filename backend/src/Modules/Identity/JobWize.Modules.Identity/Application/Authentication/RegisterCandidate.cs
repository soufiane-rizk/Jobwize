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
using System;
using System.Collections.Generic;
using System.Text;
using static JobWize.Modules.Identity.Contracts.Public.Authentication.RegisterCandidate;

namespace JobWize.Modules.Identity.Application.User
{
    public static class RegisterCandidate
    {
        public sealed record Command(
            string Email,
            string Password,
            string FirstName,
            string LastName) : ICommand<AuthenticationResponse>;

        internal sealed class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(command => command.Email)
                    .NotEmpty()
                    .EmailAddress()
                    .MaximumLength(255);

                RuleFor(command => command.Password)
                    .NotEmpty()
                    .MinimumLength(8)
                    .MaximumLength(100);

                RuleFor(command => command.FirstName)
                    .NotEmpty()
                    .MaximumLength(100);

                RuleFor(command => command.LastName)
                    .NotEmpty()
                    .MaximumLength(100);
            }
        }

        internal sealed class Endpoint : IEndpoint
        {
            public void MapEndpoint(IEndpointRouteBuilder app)
            {
                app.MapPost(
                    "/api/identity/authentication/register",
                    async (
                        Request request,
                        IDispatcher dispatcher,
                        CancellationToken cancellationToken) =>
                    {
                        var command = new Command(
                            request.Email,
                            request.Password,
                            request.FirstName,
                            request.LastName);

                        var result = await dispatcher.SendAsync(command, cancellationToken);

                        return result.ToApiResult();
                    })
                    .WithName("RegisterCandidate")
                    .WithTags("Authentication"); ;
            }
        }

        internal sealed class Handler : ICommandHandler<Command, AuthenticationResponse>
        {
            private readonly IUserRepository _userRepository;
            private readonly IPasswordHasher _passwordHasher;
            private readonly IJwtProvider _jwtProvider;
            private readonly IRefreshTokenGenerator _refreshTokenGenerator;
            private readonly IDispatcher _dispatcher;
            private readonly IClock _clock;
            private readonly JwtOptions _jwtOptions;

            public Handler(IUserRepository userRepository, IPasswordHasher passwordHasher, IJwtProvider jwtProvider, IRefreshTokenGenerator refreshTokenGenerator, IDispatcher dispatcher, IClock clock, IOptions<JwtOptions> jwtOptions)
            {
                _userRepository = userRepository;
                _passwordHasher = passwordHasher;
                _jwtProvider = jwtProvider;
                _refreshTokenGenerator = refreshTokenGenerator;
                _dispatcher = dispatcher;
                _clock = clock;
                _jwtOptions = jwtOptions.Value;
            }

            public async Task<Result<AuthenticationResponse>> HandleAsync(Command command, CancellationToken cancellationToken)
            {
                var existingUser = await _userRepository.GetByEmailAsync(command.Email, cancellationToken);

                if (existingUser is not null)
                    return Result<AuthenticationResponse>.Failure(IdentityErrors.EmailAlreadyExists(command.Email));

                var passwordHash = _passwordHasher.Hash(command.Password);

                var user = Domain.User.CreateCandidate(
                    command.Email,
                    passwordHash,
                    command.FirstName,
                    command.LastName);

                var refreshTokenExpiresAt = _clock.UtcNow.Add(_jwtOptions.RefreshTokenLifetime);

                var refreshToken = user.CreateRefreshToken(
                    _refreshTokenGenerator.Generate(),
                    refreshTokenExpiresAt);

                await _userRepository.SaveAsync(user, cancellationToken);

                var accessToken = _jwtProvider.Generate(user);

                await _dispatcher.PublishAsync(new CandidateRegistered(user.Id), cancellationToken);

                return Result<AuthenticationResponse>.Success(
                    new AuthenticationResponse(
                        user.Id,
                        user.FirstName,
                        user.LastName,
                        (UserRole)user.Role,
                        accessToken.Value,
                        refreshToken.Token,
                        accessToken.ExpiresAt,
                        refreshToken.ExpiresAt));
            }
        }
    }
}
