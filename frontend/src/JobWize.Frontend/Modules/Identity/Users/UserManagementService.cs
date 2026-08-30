using JobWize.Frontend.Shared.Api;
using JobWize.Frontend.Shared.Authentication;
using JobWize.Frontend.Shared.Results;
using JobWize.Modules.Identity.Contracts.Public.Users;
using CreateAdminContract = JobWize.Modules.Identity.Contracts.Public.Users.CreateAdmin;

namespace JobWize.Frontend.Modules.Identity.Users;

public sealed class UserManagementService(
    IHttpClientFactory httpClientFactory,
    JobWizeAuthenticationStateProvider authenticationStateProvider)
    : ApiService(httpClientFactory, authenticationStateProvider)
{
    public Task<Result<GetUsers.Response>> GetUsersAsync(CancellationToken cancellationToken = default) =>
        GetAsync<GetUsers.Request, GetUsers.Response>(GetUsers.Route, new GetUsers.Request(), cancellationToken);

    public Task<Result<CreateAdminContract.Response>> CreateAdminAsync(CreateAdminContract.Request request, CancellationToken cancellationToken = default) =>
        PostAsync<CreateAdminContract.Request, CreateAdminContract.Response>(CreateAdminContract.Route, request, cancellationToken);

    public Task<Result<bool>> SuspendAsync(SuspendUser.Request request, CancellationToken cancellationToken = default) =>
        PostAsync<SuspendUser.Request, bool>(SuspendUser.Route, request, cancellationToken);

    public Task<Result<bool>> ReactivateAsync(ReactivateUser.Request request, CancellationToken cancellationToken = default) =>
        PostAsync<ReactivateUser.Request, bool>(ReactivateUser.Route, request, cancellationToken);
}
