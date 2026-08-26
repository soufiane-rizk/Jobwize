using System.ComponentModel.DataAnnotations;
using JobWize.Modules.Identity.Domain;
using JobWize.Modules.Identity.Infrastructure.Authentication;
using JobWize.Modules.Identity.Persistence;
using JobWize.Runtime.Contracts.Transactions;

namespace JobWize.Modules.Identity.Infrastructure;

public sealed class InitialSuperAdminOptions : IValidatableObject
{
    public const string SectionName = "InitialSuperAdmin";

    public string? Email { get; init; }
    public string? TemporaryPassword { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        bool hasEmail = !string.IsNullOrWhiteSpace(Email);
        bool hasPassword = !string.IsNullOrWhiteSpace(TemporaryPassword);

        if (!hasEmail && !hasPassword)
        {
            yield break;
        }

        if (!hasEmail || !hasPassword)
        {
            yield return new ValidationResult("InitialSuperAdmin:Email and InitialSuperAdmin:TemporaryPassword must be configured together.");
            yield break;
        }

        if (!new EmailAddressAttribute().IsValid(Email))
        {
            yield return new ValidationResult("InitialSuperAdmin:Email must be a valid email address.");
        }

        if (TemporaryPassword!.Length < 8)
        {
            yield return new ValidationResult("InitialSuperAdmin:TemporaryPassword must contain at least 8 characters.");
        }
    }
}

public sealed class InitialSuperAdminBootstrapper(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    ITransactionContext transactionContext)
{
    public async Task<bool> BootstrapAsync(InitialSuperAdminOptions options, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(options);

        bool hasEmail = !string.IsNullOrWhiteSpace(options.Email);
        bool hasPassword = !string.IsNullOrWhiteSpace(options.TemporaryPassword);

        if (!hasEmail && !hasPassword)
        {
            return false;
        }

        Validator.ValidateObject(options, new ValidationContext(options), validateAllProperties: true);

        if (await userRepository.HasSuperAdminAsync(cancellationToken))
        {
            return false;
        }

        User? existingUser = await userRepository.GetByEmailAsync(options.Email!, cancellationToken);
        if (existingUser is not null)
        {
            return false;
        }

        User superAdmin = User.CreateSuperAdmin(options.Email!, passwordHasher.Hash(options.TemporaryPassword!));

        await userRepository.SaveAsync(superAdmin, cancellationToken);
        await transactionContext.PersistChangesAsync(cancellationToken);

        return true;
    }
}
