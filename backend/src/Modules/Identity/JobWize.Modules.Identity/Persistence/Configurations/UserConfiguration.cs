using JobWize.Modules.Identity.Domain;
using JobWize.Shared.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Modules.Identity.Persistence.Configurations
{
    internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users", Schemas.Identity);

            builder.ConfigureDomainModelBase();

            builder.HasQueryFilter(user => user.DeletedAt == null);

            builder.Property(user => user.Email)
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(user => user.PasswordHash)
                .IsRequired();

            builder.Property(user => user.FirstName)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(user => user.LastName)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(user => user.AvatarUrl)
                .HasMaxLength(500);

            builder.Property(user => user.Role)
                .IsRequired();

            builder.Property(user => user.Status)
                .IsRequired();

            builder.Property(user => user.MustChangePassword)
                .IsRequired();

            builder.HasIndex(user => user.Email)
                .IsUnique();

            builder.Property(user => user.Role)
                .HasConversion<string>()
                .IsRequired();

            builder.Property(user => user.Status)
                .HasConversion<string>()
                .IsRequired();

            builder.HasMany(user => user.RefreshTokens)
                .WithOne()
                .HasForeignKey(refreshToken => refreshToken.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Metadata
                .FindNavigation(nameof(User.RefreshTokens))!
                .SetPropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}
