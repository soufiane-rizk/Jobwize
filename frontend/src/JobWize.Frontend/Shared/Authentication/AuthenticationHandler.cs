using System.Net.Http.Headers;

namespace JobWize.Frontend.Shared.Authentication
{
    public sealed class AuthenticationHandler : DelegatingHandler
    {
        private readonly ITokenStorage _tokenStorage;

        public AuthenticationHandler(ITokenStorage tokenStorage)
        {
            _tokenStorage = tokenStorage;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            AuthenticationTokens? tokens = await _tokenStorage.GetAsync();

            if (tokens is not null)
            {
                request.Headers.Authorization =
                    new AuthenticationHeaderValue(
                        "Bearer",
                        tokens.AccessToken);
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
