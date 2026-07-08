using FluentValidation;

namespace IDCOL.CBS.SystemAdmin.Application.Users.Commands.AssignRole;

public sealed class AssignRoleCommandValidator : AbstractValidator<AssignRoleCommand>
{
    public AssignRoleCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.FunctionCode).NotEmpty();
        RuleFor(x => x)
            .Must(x => x.IsMaker != x.IsChecker)
            .WithMessage("A role assignment must be exactly one of Maker or Checker, never both or neither.");
    }
}
