using JobWize.Modules.Identity.Contracts.Public.Authentication;
using System.Net.Http.Json;

namespace JobWize.Frontend.Shared.Authentication
{
    public sealed class TokenRefreshService
    {
        private static readonly SemaphoreSlim RefreshLock = new(1, 1);
        private readonly HttpClient _httpClient;
        private readonly ITokenStorage _tokenStorage;
        private readonly JobWizeAuthenticationStateProvider _authenticationStateProvider;

        public TokenRefreshService(IHttpClientFactory httpClientFactory, ITokenStorage tokenStorage, JobWizeAuthenticationStateProvider authenticationStateProvider)
        {
            _httpClient = httpClientFactory.CreateClient("AnonymousApi");
            _tokenStorage = tokenStorage;
            _authenticationStateProvider = authenticationStateProvider;
        }

        public async Task<AuthenticationTokens?> RefreshAsync(AuthenticationTokens failedTokens, CancellationToken cancellationToken)
        {
            await RefreshLock.WaitAsync(cancellationToken);

            try
            {
                AuthenticationTokens? currentTokens = await _tokenStorage.GetAsync(cancellationToken);

                if (currentTokens is null)
                    return null;

                if (currentTokens.RefreshToken != failedTokens.RefreshToken)
                    return currentTokens;

                HttpResponseMessage response = await _httpClient.PostAsJsonAsync(
                    Refresh.Route,
                    new Refresh.Request(currentTokens.RefreshToken),
                    cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    await _authenticationStateProvider.LogoutAsync();
                    return null;
                }

                AuthenticationResponse? authentication = await response.Content.ReadFromJsonAsync<AuthenticationResponse>(cancellationToken);

                if (authentication is null)
                {
                    await _authenticationStateProvider.LogoutAsync();
                    return null;
                }

                AuthenticationTokens replacementTokens = new(authentication.AccessToken, authentication.RefreshToken);
                await _authenticationStateProvider.AuthenticateAsync(replacementTokens);

                return replacementTokens;
            }
            catch (HttpRequestException)
            {
                await _authenticationStateProvider.LogoutAsync();
                return null;
            }
            finally
            {
                RefreshLock.Release();
            }
        }
    }
}
