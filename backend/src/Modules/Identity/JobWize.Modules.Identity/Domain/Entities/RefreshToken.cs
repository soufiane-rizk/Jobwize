using JobWize.Shared.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Modules.Identity.Domain.Entities
{
    public sealed class RefreshToken : Entity
    {
        public Guid UserId { get; private set; }

        public string TokenHash { get; private set; } = default!;

        public DateTime ExpiresAt { get; private set; }

        public DateTime? RevokedAt { get; private set; }

        private RefreshToken()
        {
        }

        public static RefreshToken Create(Guid userId, string tokenHash, DateTime expiresAt)
        {
            return new RefreshToken
            {
                UserId = userId,
                TokenHash = tokenHash,
                ExpiresAt = expiresAt
            };
        }

        public void Revoke(DateTime revokedAt)
        {
            RevokedAt = revokedAt;
        }

        public bool IsExpired(DateTime utcNow) => ExpiresAt <= utcNow;

        public bool IsRevoked => RevokedAt is not null;

        public bool IsActive(DateTime utcNow) => !IsExpired(utcNow) && !IsRevoked;
    }
}
