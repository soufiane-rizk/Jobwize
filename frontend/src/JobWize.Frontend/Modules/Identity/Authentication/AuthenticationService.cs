using JobWize.Frontend.Shared.Api;
using JobWize.Frontend.Shared.Authentication;
using JobWize.Frontend.Shared.Results;
using JobWize.Modules.Identity.Contracts.Public.Authentication;

using LoginContract = JobWize.Modules.Identity.Contracts.Public.Authentication.Login;
using RegisterCandidateContract = JobWize.Modules.Identity.Contracts.Public.Authentication.RegisterCandidate;

namespace JobWize.Frontend.Modules.Identity.Authentication
{
    public class AuthenticationService : ApiService
    {
        private readonly JobWizeAuthenticationStateProvider _authenticationStateProvider;

        public AuthenticationService(IHttpClientFactory httpClientFactory, JobWizeAuthenticationStateProvider authenticationStateProvider)
            : base(httpClientFactory, authenticationStateProvider)
        {
            _authenticationStateProvider = authenticationStateProvider;
        }

        public async Task<Result<AuthenticationResponse>> LoginAsync(string username, string password, CancellationToken cancellationToken = default)
        {
            var request = new LoginContract.Request(username, password);

            var result = await PostAsync<LoginContract.Request, AuthenticationResponse>(LoginContract.Route, request, cancellationToken);

            if (result.IsSuccess && result.Value != null)
            {
                await _authenticationStateProvider.AuthenticateAsync(
                    new AuthenticationTokens(
                        result.Value.AccessToken,
                        result.Value.RefreshToken));
            }

            return result;
        }

        public async Task<Result<AuthenticationResponse>> RegisterCandidateAsync(RegisterCandidateContract.Request request, CancellationToken cancellationToken = default)
        {
            var result = await PostAsync<RegisterCandidateContract.Request, AuthenticationResponse>(RegisterCandidateContract.Route, request, cancellationToken);

            if (result.IsSuccess && result.Value != null)
            {
                await _authenticationStateProvider.AuthenticateAsync(
                    new AuthenticationTokens(
                        result.Value.AccessToken,
                        result.Value.RefreshToken));
            }

            return result;
        }

        public async Task<Result> LogoutAsync(CancellationToken cancellationToken = default)
        {
            string? refreshToken = await _authenticationStateProvider.GetRefreshTokenAsync();
            if (refreshToken is null)
            {
                await _authenticationStateProvider.LogoutAsync();
                return Result.Success();
            }

            try
            {
                var request = new Logout.Request(refreshToken);

                return await PostAsync("/api/identity/authentication/logout", request, cancellationToken);
            }
            finally
            {
                // A local logout must not depend on the API being reachable.
                // The server-side session is revoked whenever the request succeeds.
                await _authenticationStateProvider.LogoutAsync();
            }
        }
    }
}
