using FluentValidation;
using IDCOL.CBS.BuildingBlocks.Application.Abstractions;
using IDCOL.CBS.Collection.Application.Abstractions;
using IDCOL.CBS.Collection.Domain;
using MediatR;

namespace IDCOL.CBS.Collection.Application.Commands;

/// <summary>Stage 1 (CAD): record a received repayment with its Principal/Interest/LPC breakdown.</summary>
public sealed record EnterReceiptCommand(
    Guid SanctionId,
    string SanctionRef,
    string CustomerNo,
    string ProjectName,
    string Currency,
    string PaymentMode,
    string? InstrumentNo,
    string? BankName,
    decimal InstrumentAmount,
    DateOnly ValueDate,
    DateOnly ReceiveDate,
    DateOnly? LpcDate,
    decimal PrincipalAmount,
    decimal InterestAmount,
    decimal LpcAmount) : IRequest<Guid>, IAuditableAction, ITransactionalCommand;

public sealed class EnterReceiptCommandValidator : AbstractValidator<EnterReceiptCommand>
{
    public EnterReceiptCommandValidator()
    {
        RuleFor(x => x.SanctionId).NotEmpty();
        RuleFor(x => x.PaymentMode).NotEmpty();
        RuleFor(x => x.InstrumentAmount).GreaterThan(0);
        RuleFor(x => x.PrincipalAmount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.InterestAmount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.LpcAmount).GreaterThanOrEqualTo(0);
        RuleFor(x => x)
            .Must(x => Math.Round(x.PrincipalAmount + x.InterestAmount + x.LpcAmount - x.InstrumentAmount, 2) == 0)
            .WithMessage("Principal + Interest + LPC must equal the instrument amount.");
    }
}

public sealed class EnterReceiptCommandHandler : IRequestHandler<EnterReceiptCommand, Guid>
{
    private readonly IReceiptRepository _repository;
    private readonly ICurrentUserAccessor _currentUser;

    public EnterReceiptCommandHandler(IReceiptRepository repository, ICurrentUserAccessor currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(EnterReceiptCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsInRole("CAD"))
            throw new UnauthorizedAccessException("Only a CAD user can enter a collection.");

        var id = Guid.NewGuid();
        var referenceNo = $"CO-{DateTime.UtcNow:yyyyMMdd}-{id.ToString()[..8]}";
        var receipt = Receipt.Enter(
            id, referenceNo, request.SanctionId, request.SanctionRef, request.CustomerNo, request.ProjectName,
            request.Currency, request.PaymentMode, request.InstrumentNo, request.BankName, request.InstrumentAmount,
            request.ValueDate, request.ReceiveDate, request.LpcDate, request.PrincipalAmount, request.InterestAmount,
            request.LpcAmount, _currentUser.UserId);

        await _repository.AddAsync(receipt, cancellationToken);
        return id;
    }
}
