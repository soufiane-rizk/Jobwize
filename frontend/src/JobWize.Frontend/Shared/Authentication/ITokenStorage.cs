namespace JobWize.Frontend.Shared.Authentication
{
    public interface ITokenStorage
    {
        Task<AuthenticationTokens?> GetAsync(CancellationToken cancellationToken = default);

        Task SaveAsync(AuthenticationTokens tokens, CancellationToken cancellationToken = default);

        Task ClearAsync(CancellationToken cancellationToken = default);
    }
}
