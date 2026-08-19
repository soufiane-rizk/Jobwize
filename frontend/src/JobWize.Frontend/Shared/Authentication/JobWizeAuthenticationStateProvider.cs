using Microsoft.AspNetCore.Components.Authorization;

namespace JobWize.Frontend.Shared.Authentication
{
    public sealed class JobWizeAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly ITokenStorage _tokenStorage;


        public JobWizeAuthenticationStateProvider(ITokenStorage tokenStorage)
        {
            _tokenStorage = tokenStorage;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            AuthenticationTokens? tokens = await _tokenStorage.GetAsync();

            if (tokens is null)
            {
                return new AuthenticationState(JwtParser.Anonymous);
            }

            return new AuthenticationState(JwtParser.Parse(tokens.AccessToken));
        }

        public async Task<string?> GetRefreshTokenAsync()
        {
            AuthenticationTokens? tokens = await _tokenStorage.GetAsync();

            return tokens?.RefreshToken;
        }

        public async Task AuthenticateAsync(AuthenticationTokens tokens)
        {
            await _tokenStorage.SaveAsync(tokens);

            NotifyAuthenticationChanged();
        }

        public async Task LogoutAsync()
        {
            await _tokenStorage.ClearAsync();

            NotifyAuthenticationChanged();
        }

        private void NotifyAuthenticationChanged()
        {
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
    }
}
