using System.Security.Cryptography;
using System.Text;

namespace JobWize.Modules.Identity.Infrastructure.Authentication
{
    public interface IRefreshTokenHasher
    {
        string Hash(string token);
    }

    internal sealed class RefreshTokenHasher : IRefreshTokenHasher
    {
        public string Hash(string token)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(token);

            return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
        }
    }
}
