using JobWize.Modules.Identity.Domain.Entities;
using JobWize.Modules.Identity.Domain.Enums;
using JobWize.Shared.Domain;
using JobWize.Shared.Errors;
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
            EnsureRequired(email);
            EnsureRequired(passwordHash);
            EnsureRequired(firstName);
            EnsureRequired(lastName);

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

        public static User CreateSuperAdmin(string email, string passwordHash)
        {
            EnsureRequired(email);
            EnsureRequired(passwordHash);

            return new User
            {
                Id = Guid.NewGuid(),
                Email = email,
                PasswordHash = passwordHash,
                FirstName = "Initial",
                LastName = "SuperAdmin",
                Role = UserRole.SuperAdmin,
                Status = UserStatus.Active,
                MustChangePassword = true
            };
        }

        public RefreshToken CreateRefreshToken(string tokenHash, DateTime expiresAt)
        {
            var refreshToken = RefreshToken.Create(Id, tokenHash, expiresAt);

            _refreshTokens.Add(refreshToken);

            return refreshToken;
        }

        public static User CreateAdmin(string email, string passwordHash, string firstName, string lastName)
        {
            EnsureRequired(email);
            EnsureRequired(passwordHash);
            EnsureRequired(firstName);
            EnsureRequired(lastName);

            return new User { Id = Guid.NewGuid(), Email = email, PasswordHash = passwordHash, FirstName = firstName, LastName = lastName, Role = UserRole.Admin, Status = UserStatus.Active, MustChangePassword = true };
        }

        public void Suspend(DateTime revokedAt)
        {
            Status = UserStatus.Suspended;
            RevokeAllRefreshTokens(revokedAt);
        }

        public void Reactivate() => Status = UserStatus.Active;

        public void UpdatePersonalInformation(string firstName, string lastName)
        {
            EnsureRequired(firstName);
            EnsureRequired(lastName);

            FirstName = firstName;
            LastName = lastName;
        }

        public RefreshToken? FindRefreshToken(string tokenHash)
        {
            return _refreshTokens.SingleOrDefault(x => x.TokenHash == tokenHash);
        }

        public void ChangePassword(string passwordHash)
        {
            EnsureRequired(passwordHash);

            PasswordHash = passwordHash;
            MustChangePassword = false;
        }

        public void RevokeAllRefreshTokens(DateTime revokedAt)
        {
            foreach (RefreshToken refreshToken in _refreshTokens.Where(token => !token.IsRevoked))
            {
                refreshToken.Revoke(revokedAt);
            }
        }

        public void RevokeRefreshToken(string tokenHash, DateTime revokedAt)
        {
            RefreshToken? refreshToken = FindRefreshToken(tokenHash);

            if (refreshToken is null)
            {
                return;
            }

            refreshToken.Revoke(revokedAt);
        }

        private static void EnsureRequired(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new BusinessRuleException(DomainErrors.RequiredValue);
            }
        }
    }
}
