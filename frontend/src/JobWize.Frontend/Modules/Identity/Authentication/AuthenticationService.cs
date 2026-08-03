using JobWize.Frontend.Shared.Api;
using JobWize.Frontend.Shared.Results;
using JobWize.Modules.Identity.Contracts.Public.Authentication;

using LoginContract = JobWize.Modules.Identity.Contracts.Public.Authentication.Login;
using RegisterCandidateContract = JobWize.Modules.Identity.Contracts.Public.Authentication.RegisterCandidate;

namespace JobWize.Frontend.Modules.Identity.Authentication
{
    public class AuthenticationService : ApiService
    {
        public AuthenticationService(HttpClient httpClient)
            : base(httpClient)
        {
        }
    

        public Task<Result<AuthenticationResponse>> LoginAsync(string username, string password, CancellationToken cancellationToken = default)
        {
            var request = new LoginContract.Request(username, password);

            return PostAsync<LoginContract.Request, AuthenticationResponse>(
                LoginContract.Route,
                request,
                cancellationToken);
        }

        public Task<Result<AuthenticationResponse>> RegisterCandidateAsync(RegisterCandidateContract.Request request, CancellationToken cancellationToken = default)
        {
            return PostAsync<RegisterCandidateContract.Request, AuthenticationResponse>(
                RegisterCandidateContract.Route,
                request,
                cancellationToken);
        }

        public Task<Result> LogoutAsync(CancellationToken cancellationToken = default)
        {
            return PostAsync(
                "/api/identity/authentication/logout",
                new { },
                cancellationToken);
        }
    }
}