using JobWize.Modules.Identity.Domain.Entities;
using JobWize.Shared.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Modules.Identity.Persistence.Configurations
{
    internal sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.ConfigureEntityBase();

            builder.ToTable("RefreshTokens", Schemas.Identity);

            builder.Property(refreshToken => refreshToken.Token)
                .HasMaxLength(512)
                .IsRequired();

            builder.Property(refreshToken => refreshToken.ExpiresAt)
                .IsRequired();

            builder.Property(refreshToken => refreshToken.RevokedAt);

            builder.HasIndex(refreshToken => refreshToken.Token)
                .IsUnique();

            builder.HasIndex(refreshToken => refreshToken.UserId);
        }
    }
}
