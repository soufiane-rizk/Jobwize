using JobWize.Modules.Identity.Contracts.Public.Authentication;
using System.Net;
using System.Net.Http.Headers;

namespace JobWize.Frontend.Shared.Authentication
{
    public sealed class AuthenticationHandler : DelegatingHandler
    {
        private readonly ITokenStorage _tokenStorage;
        private readonly TokenRefreshService _tokenRefreshService;

        public AuthenticationHandler(ITokenStorage tokenStorage, TokenRefreshService tokenRefreshService)
        {
            _tokenStorage = tokenStorage;
            _tokenRefreshService = tokenRefreshService;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (IsAuthenticationRequest(request))
                return await base.SendAsync(request, cancellationToken);

            AuthenticationTokens? tokens = await _tokenStorage.GetAsync(cancellationToken);

            if (tokens is null)
                return await base.SendAsync(request, cancellationToken);

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);
            HttpRequestMessage retryRequest = await CloneAsync(request, cancellationToken);

            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

            if (response.StatusCode != HttpStatusCode.Unauthorized)
                return response;

            AuthenticationTokens? replacementTokens = await _tokenRefreshService.RefreshAsync(tokens, cancellationToken);

            if (replacementTokens is null)
                return response;

            response.Dispose();
            retryRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", replacementTokens.AccessToken);

            return await base.SendAsync(retryRequest, cancellationToken);
        }

        private static bool IsAuthenticationRequest(HttpRequestMessage request)
        {
            string? path = request.RequestUri?.AbsolutePath;

            return path is Login.Route or Logout.Route or Refresh.Route or RegisterCandidate.Route;
        }

        private static async Task<HttpRequestMessage> CloneAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var clone = new HttpRequestMessage(request.Method, request.RequestUri);

            foreach (KeyValuePair<string, IEnumerable<string>> header in request.Headers)
                clone.Headers.TryAddWithoutValidation(header.Key, header.Value);

            if (request.Content is not null)
            {
                byte[] content = await request.Content.ReadAsByteArrayAsync(cancellationToken);
                clone.Content = new ByteArrayContent(content);

                foreach (KeyValuePair<string, IEnumerable<string>> header in request.Content.Headers)
                    clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            return clone;
        }
    }
}
