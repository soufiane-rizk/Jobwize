using FluentValidation;
using JobWize.Modules.Identity.Application;
using JobWize.Modules.Identity.Contracts.Events.Authentication;
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
using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Modules.Identity.Application.Authentication
{
    public static class Logout
    {
        internal sealed record Command(string RefreshToken) : ICommand<bool>;

        internal sealed class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(x => x.RefreshToken)
                    .NotEmpty();
            }
        }

        internal sealed class Endpoint : IEndpoint
        {
            public void MapEndpoint(IEndpointRouteBuilder app)
            {
                app.MapPost(
                    Contracts.Public.Authentication.Logout.Route,
                    async (Contracts.Public.Authentication.Logout.Request request, IDispatcher dispatcher, CancellationToken cancellationToken) =>
                    {
                        var result = await dispatcher.SendAsync(
                            new Command(request.RefreshToken),
                            cancellationToken);

                        return result.ToApiResult();
                    })
                    .RequireAuthorization()
                    .WithName("Logout")
                    .WithTags("Authentication");
            }
        }

        internal sealed class Handler : ICommandHandler<Command, bool>
        {
            private readonly IUserRepository _userRepository;
            private readonly IUserContext _userContext;
            private readonly IClock _clock;
            private readonly IDispatcher _dispatcher;

            public Handler(IUserRepository userRepository,  IUserContext userContext, IClock clock, IDispatcher dispatcher)
            {
                _userRepository = userRepository;
                _userContext = userContext;
                _clock = clock;
                _dispatcher = dispatcher;
            }

            public async Task<Result<bool>> HandleAsync(Command command, CancellationToken cancellationToken)
            {
                var user = await _userRepository.GetByIdAsync(_userContext.UserId, cancellationToken);

                if (user is null)
                {
                    return Result<bool>.Failure(IdentityErrors.UserNotFound);
                }

                user.RevokeRefreshToken(command.RefreshToken, _clock.UtcNow);

                await _userRepository.SaveAsync(user, cancellationToken);

                await _dispatcher.PublishAsync(new UserLoggedOut(user.Id), cancellationToken);

                return Result<bool>.Success(true);
            }
        }
    }
}
