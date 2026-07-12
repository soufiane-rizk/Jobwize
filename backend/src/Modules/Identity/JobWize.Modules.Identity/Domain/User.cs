using JobWize.Modules.Identity.Domain.Entities;
using JobWize.Modules.Identity.Domain.Enums;
using JobWize.Shared.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Modules.Identity.Domain
{
    public sealed class User : DomainModel
    {
        // Identity
        public string Email { get; private set; } = default!;
        public string PasswordHash { get; private set; } = default!;

        // Personal Information
        public string FirstName { get; private set; } = default!;
        public string LastName { get; private set; } = default!;
        public string? AvatarUrl { get; private set; }

        // Authorization
        public UserRole Role { get; private set; }
        public UserStatus Status { get; private set; }
        public bool MustChangePassword { get; private set; }

        // Sessions
        private readonly List<RefreshToken> _refreshTokens = [];
        public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();

        private User() { }

        public static User CreateCandidate(
            string email,
            string passwordHash,
            string firstName,
            string lastName)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(email, nameof(email));
            ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash, nameof(passwordHash));
            ArgumentException.ThrowIfNullOrWhiteSpace(firstName, nameof(firstName));
            ArgumentException.ThrowIfNullOrWhiteSpace(lastName, nameof(lastName));

            return new User
            {
                Id = Guid.NewGuid(),
                Email = email,
                PasswordHash = passwordHash,

                FirstName = firstName,
                LastName = lastName,

                Role = UserRole.Candidate,
                Status = UserStatus.Active,

                MustChangePassword = false
            };
        }

        public RefreshToken CreateRefreshToken(string token, DateTime expiresAt)
        {
            var refreshToken = RefreshToken.Create(Id, token, expiresAt);

            _refreshTokens.Add(refreshToken);

            return refreshToken;
        }
    }
}
