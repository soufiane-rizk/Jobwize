using JobWize.Modules.Identity.Domain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Modules.Identity.Persistence
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default);

        Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

        Task SaveAsync(User user, CancellationToken cancellationToken = default);
    }

    internal sealed class UserRepository : IUserRepository
    {
        private readonly IdentityDbContext _dbContext;

        public UserRepository(IdentityDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<User?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Users.FindAsync([userId], cancellationToken);
        }

        public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Users
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(user => user.Email == email, cancellationToken);
        }

        public Task SaveAsync(User user, CancellationToken cancellationToken = default)
        {
            if (_dbContext.Entry(user).State == EntityState.Detached)
            {
                _dbContext.Users.Add(user);
            }

            return Task.CompletedTask;
        }
    }
}
