using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace JobWize.Modules.Identity.Infrastructure.Authentication
{
    public sealed class JwtOptions
    {
        public const string SectionName = "Jwt";

        [Required]
        public string Issuer { get; init; } = default!;

        [Required]
        public string Audience { get; init; } = default!;

        [Required]
        [MinLength(32)]
        public string SecretKey { get; init; } = default!;

        [Required]
        public TimeSpan AccessTokenLifetime { get; init; }

        [Required]
        public TimeSpan RefreshTokenLifetime { get; init; }
    }
}
