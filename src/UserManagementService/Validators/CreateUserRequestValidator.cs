using FluentValidation;
using UserManagementService.Models.DTOs;

namespace UserManagementService.Validators;

/// <summary>
/// Validator for CreateUserRequest.
/// </summary>
public class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(255).WithMessage("Name must not exceed 255 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.")
            .MaximumLength(255).WithMessage("Email must not exceed 255 characters.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters.")
            .MaximumLength(100).WithMessage("Password must not exceed 100 characters.")
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least one digit.");

        RuleFor(x => x.Role)
            .MaximumLength(50).WithMessage("Role must not exceed 50 characters.")
            .When(x => !string.IsNullOrEmpty(x.Role));
    }
}

/// <summary>
/// Validator for UpdateUserRequest.
/// </summary>
public class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
{
    public UpdateUserRequestValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(255).WithMessage("Name must not exceed 255 characters.")
            .When(x => !string.IsNullOrEmpty(x.Name));

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Invalid email format.")
            .MaximumLength(255).WithMessage("Email must not exceed 255 characters.")
            .When(x => !string.IsNullOrEmpty(x.Email));

        RuleFor(x => x.Role)
            .MaximumLength(50).WithMessage("Role must not exceed 50 characters.")
            .When(x => !string.IsNullOrEmpty(x.Role));
    }
}

/// <summary>
/// Validator for LoginRequest.
/// </summary>
public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.");
    }
}

/// <summary>
/// Validator for VerificationRequest.
/// </summary>
public class VerificationRequestValidator : AbstractValidator<VerificationRequest>
{
    public VerificationRequestValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Verification type is required.")
            .Must(type => type == "email" || type == "password_reset")
            .WithMessage("Verification type must be 'email' or 'password_reset'.");
    }
}

/// <summary>
/// Validator for ValidateVerificationRequest.
/// </summary>
public class ValidateVerificationRequestValidator : AbstractValidator<ValidateVerificationRequest>
{
    public ValidateVerificationRequestValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Verification code is required.")
            .Length(6).WithMessage("Verification code must be 6 digits.");

        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Verification type is required.")
            .Must(type => type == "email" || type == "password_reset")
            .WithMessage("Verification type must be 'email' or 'password_reset'.");
    }
}

/// <summary>
/// Validator for RefreshTokenRequest.
/// </summary>
public class RefreshTokenRequestValidator : AbstractValidator<RefreshTokenRequest>
{
    public RefreshTokenRequestValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token is required.");
    }
}
