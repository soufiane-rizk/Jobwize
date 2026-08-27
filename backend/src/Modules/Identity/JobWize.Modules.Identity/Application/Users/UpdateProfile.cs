using FluentValidation;
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

public static class UpdateProfile
{
    internal sealed record Command(string FirstName, string LastName) : ICommand<bool>;

    internal sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(command => command.FirstName).NotEmpty().MaximumLength(100);
            RuleFor(command => command.LastName).NotEmpty().MaximumLength(100);
        }
    }

    internal sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPut(
                    Contracts.Public.Users.UpdateProfile.Route,
                    async (Contracts.Public.Users.UpdateProfile.Request request, IDispatcher dispatcher, CancellationToken cancellationToken) =>
                    {
                        Result<bool> result = await dispatcher.SendAsync(
                            new Command(request.FirstName, request.LastName),
                            cancellationToken);

                        return result.ToApiResult();
                    })
                .RequireAuthorization()
                .WithName("UpdateProfile")
                .WithTags("Users");
        }
    }

    internal sealed class Handler(IUserRepository userRepository, IUserContext userContext) : ICommandHandler<Command, bool>
    {
        public async Task<Result<bool>> HandleAsync(Command command, CancellationToken cancellationToken)
        {
            Domain.User? user = await userRepository.GetByIdAsync(userContext.UserId, cancellationToken);
            if (user is null)
            {
                return Result<bool>.Failure(IdentityErrors.UserNotFound);
            }

            user.UpdatePersonalInformation(command.FirstName, command.LastName);

            return Result<bool>.Success(true);
        }
    }
}
