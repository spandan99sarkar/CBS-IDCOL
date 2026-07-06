using FluentValidation;

namespace IDCOL.CBS.SystemAdmin.Application.Users.Commands.CreateUser;

public sealed class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Username).NotEmpty().MaximumLength(50);
        RuleFor(x => x.DisplayName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();

        // Bangladesh Bank ICT Security Guideline: minimum 11 characters when MFA is not in use,
        // with at least 3 of {uppercase, lowercase, digit, special char}. MFA itself is a
        // later-phase concern; the length/complexity floor is enforced here regardless.
        RuleFor(x => x.PlainTextPassword)
            .NotEmpty()
            .MinimumLength(11)
            .Must(HaveSufficientComplexity)
            .WithMessage("Password must contain at least 3 of: uppercase, lowercase, digit, special character.");

        RuleFor(x => x.BusinessUnitCode).NotEmpty();
    }

    private static bool HaveSufficientComplexity(string password)
    {
        var categories = 0;
        if (password.Any(char.IsUpper)) categories++;
        if (password.Any(char.IsLower)) categories++;
        if (password.Any(char.IsDigit)) categories++;
        if (password.Any(c => !char.IsLetterOrDigit(c))) categories++;
        return categories >= 3;
    }
}
