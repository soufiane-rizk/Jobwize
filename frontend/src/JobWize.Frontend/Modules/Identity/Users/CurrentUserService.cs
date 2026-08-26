using JobWize.Frontend.Shared.Api;
using JobWize.Frontend.Shared.Authentication;
using JobWize.Frontend.Shared.Results;
using JobWize.Modules.Identity.Contracts.Public.Users;

namespace JobWize.Frontend.Modules.Identity.Users
{
    public sealed class CurrentUserService : ApiService
    {
        public CurrentUserService(
            IHttpClientFactory httpClientFactory,
            JobWizeAuthenticationStateProvider authenticationStateProvider)
            : base(httpClientFactory, authenticationStateProvider)
        {
        }

        public Task<Result<GetCurrentUser.Response>> GetAsync(CancellationToken cancellationToken = default)
        {
            return GetAsync<GetCurrentUser.Request, GetCurrentUser.Response>(
                GetCurrentUser.Route,
                new GetCurrentUser.Request(),
                cancellationToken);
        }
    }
}
