
using Microsoft.JSInterop;
using System.Reflection;

namespace JobWize.Frontend.Shared.Authentication
{
    internal sealed class LocalStorageTokenStorage : ITokenStorage
    {
        private readonly IJSRuntime _jsRuntime;

        public LocalStorageTokenStorage(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task<AuthenticationTokens?> GetAsync(CancellationToken cancellationToken = default)
        {
            string? accessToken = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", cancellationToken, StorageKeys.AccessToken);

            string? refreshToken = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", cancellationToken, StorageKeys.RefreshToken);

            if (string.IsNullOrWhiteSpace(accessToken) || string.IsNullOrWhiteSpace(refreshToken))
            {
                return null;
            }

            return new AuthenticationTokens(accessToken, refreshToken);
        }

        public async Task SaveAsync(AuthenticationTokens tokens, CancellationToken cancellationToken = default)
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", cancellationToken, StorageKeys.AccessToken, tokens.AccessToken);

            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", cancellationToken, StorageKeys.RefreshToken, tokens.RefreshToken);
        }

        public async Task ClearAsync(CancellationToken cancellationToken = default)
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", cancellationToken, StorageKeys.AccessToken);

            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", cancellationToken, StorageKeys.RefreshToken);
        }
    }
}
