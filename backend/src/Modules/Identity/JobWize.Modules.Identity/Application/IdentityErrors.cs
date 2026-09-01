using JobWize.Shared.Application.Results;
using JobWize.Shared.Errors;
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

        public static Error InvalidCredentials =
            new(
                $"{Prefix}.InvalidCredentials",
                "The email or password is incorrect.",
                ErrorType.Unauthorized);

        public static readonly Error InvalidCurrentPassword =
            new(
                $"{Prefix}.InvalidCurrentPassword",
                "The current password is incorrect.",
                ErrorType.Validation);

        public static readonly Error AccountSuspended =
            new($"{Prefix}.AccountSuspended", "This account is suspended.", ErrorType.Forbidden);

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

        public static readonly Error UserManagementForbidden =
            new($"{Prefix}.UserManagementForbidden", "You are not allowed to manage this user.", ErrorType.Forbidden);
    }
}
