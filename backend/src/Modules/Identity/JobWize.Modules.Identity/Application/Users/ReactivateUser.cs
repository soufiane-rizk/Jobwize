using JobWize.Modules.Identity.Contracts.Events.Users;
using JobWize.Modules.Identity.Contracts.Public.Authentication;
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

public static class ReactivateUser
{
    internal sealed record Command(Guid UserId, bool IsSuperAdmin) : ICommand<bool>;

    internal sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost(
                    Contracts.Public.Users.ReactivateUser.Route,
                    async (
                        Contracts.Public.Users.ReactivateUser.Request request,
                        HttpContext context,
                        IDispatcher dispatcher,
                        CancellationToken cancellationToken) =>
                    {
                        Result<bool> result = await dispatcher.SendAsync(
                            new Command(request.UserId, context.User.IsInRole("SuperAdmin")),
                            cancellationToken);

                        return result.ToApiResult();
                    })
                .RequireAuthorization(AuthenticationPolicies.UserManagement)
                .WithName("ReactivateUser")
                .WithTags("Users");
        }
    }

    internal sealed class Handler(IUserRepository users, IUserContext currentUser, IDispatcher dispatcher) : ICommandHandler<Command, bool>
    {
        public async Task<Result<bool>> HandleAsync(Command command, CancellationToken ct)
        {
            Domain.User? user = await users.GetByIdAsync(command.UserId, ct);
            if (user is null)
            {
                return Result<bool>.Failure(IdentityErrors.UserNotFound);
            }

            if (!CanManage(user, command.IsSuperAdmin))
            {
                return Result<bool>.Failure(IdentityErrors.UserManagementForbidden);
            }

            if (user.Status == Domain.Enums.UserStatus.Active)
            {
                return Result<bool>.Success(true);
            }

            user.Reactivate();
            await dispatcher.PublishAsync(new UserReactivated(user.Id, currentUser.UserId), ct);

            return Result<bool>.Success(true);
        }
    }

    private static bool CanManage(Domain.User user, bool isSuperAdmin) =>
        user.Role == Domain.Enums.UserRole.Candidate ||
        (isSuperAdmin && user.Role == Domain.Enums.UserRole.Admin);
}
