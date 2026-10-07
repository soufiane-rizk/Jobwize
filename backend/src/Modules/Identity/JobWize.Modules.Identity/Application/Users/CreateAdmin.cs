using FluentValidation;
using JobWize.Modules.Identity.Contracts.Events.Users;
using JobWize.Modules.Identity.Contracts.Public.Authentication;
using JobWize.Modules.Identity.Infrastructure.Authentication;
using JobWize.Modules.Identity.Persistence;
using JobWize.Runtime.Contracts.Dispatching;
using JobWize.Shared.Application.Results;
using JobWize.Shared.Application.Security;
using JobWize.Shared.Endpoints;
using JobWize.Shared.Runtime.Contracts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace JobWize.Modules.Identity.Application.Users;

public static class CreateAdmin
{
    internal sealed record Command(string Email, string TemporaryPassword, string FirstName, string LastName) : ICommand<Contracts.Public.Users.CreateAdmin.Response>;

    internal sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.TemporaryPassword).NotEmpty().MinimumLength(8).MaximumLength(100);
            RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
            RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        }
    }

    internal sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost(
                    Contracts.Public.Users.CreateAdmin.Route,
                    async (
                        Contracts.Public.Users.CreateAdmin.Request request,
                        IDispatcher dispatcher,
                        CancellationToken cancellationToken) =>
                    {
                        Result<Contracts.Public.Users.CreateAdmin.Response> result = await dispatcher.SendAsync(
                            new Command(request.Email, request.TemporaryPassword, request.FirstName, request.LastName),
                            cancellationToken);

                        return result.ToApiResult();
                    })
                .RequireAuthorization(AuthenticationPolicies.SuperAdmin)
                .WithName("CreateAdmin")
                .WithTags("Users");
        }
    }

    internal sealed class Handler(IUserRepository users, IPasswordHasher passwords, IUserContext currentUser, IDispatcher dispatcher) : ICommandHandler<Command, Contracts.Public.Users.CreateAdmin.Response>
    {
        public async Task<Result<Contracts.Public.Users.CreateAdmin.Response>> HandleAsync(Command command, CancellationToken ct)
        {
            if (await users.GetByEmailAsync(command.Email, ct) is not null)
            {
                return Result<Contracts.Public.Users.CreateAdmin.Response>.Failure(
                    IdentityErrors.EmailAlreadyExists(command.Email));
            }

            Domain.User user = Domain.User.CreateAdmin(
                command.Email,
                passwords.Hash(command.TemporaryPassword),
                command.FirstName,
                command.LastName);

            await users.SaveAsync(user, ct);
            await dispatcher.PublishAsync(new UserCreated(user.Id, currentUser.UserId), ct);

            return Result<Contracts.Public.Users.CreateAdmin.Response>.Success(new(user.Id));
        }
    }
}
