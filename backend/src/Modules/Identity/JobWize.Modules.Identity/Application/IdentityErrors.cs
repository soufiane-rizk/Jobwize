using JobWize.Shared.Application.Results;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Modules.Identity.Application
{
    public static class IdentityErrors
    {
        private const string Prefix = "Identity";

        public static Error EmailAlreadyExists(string email) =>
            new(
                $"{Prefix}.EmailAlreadyExists",
                $"The email is already registered.",
                ErrorType.Conflict);

        public static readonly Error InvalidCredentials =
            new(
                $"{Prefix}.InvalidCredentials",
                "The email or password is incorrect.",
                ErrorType.Unauthorized);

        public static readonly Error RefreshTokenExpired =
            new(
                $"{Prefix}.RefreshTokenExpired",
                "The refresh token has expired.",
                ErrorType.Unauthorized);

        public static readonly Error RefreshTokenInvalid =
            new(
                $"{Prefix}.RefreshTokenInvalid",
                "The refresh token is invalid.",
                ErrorType.Unauthorized);

        public static readonly Error UserNotFound =
            new(
                $"{Prefix}.UserNotFound",
                "The requested user was not found.",
                ErrorType.NotFound);
    }
}
