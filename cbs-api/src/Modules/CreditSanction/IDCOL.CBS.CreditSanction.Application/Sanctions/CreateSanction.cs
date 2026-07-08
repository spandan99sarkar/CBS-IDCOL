using FluentValidation;
using IDCOL.CBS.BuildingBlocks.Application.Abstractions;
using IDCOL.CBS.CreditSanction.Application.Abstractions;
using IDCOL.CBS.CreditSanction.Domain;
using MediatR;

namespace IDCOL.CBS.CreditSanction.Application.Sanctions;

public sealed record CreateSanctionCommand(
    string SanctionId,
    Guid CustomerId,
    string CustomerNo,
    string ProductCode,
    string ProjectName,
    string? IndustryType,
    string LoanCurrency,
    decimal LoanAmount,
    string GrantCurrency,
    decimal GrantAmount,
    DateOnly AgreementDate,
    DateOnly? ExpiryDate,
    string InterestRateType,
    decimal InitialInterestRatePercent,
    int LoanTenorMonths,
    int NoOfPrincipalRepayments,
    int InterestGracePeriodMonths,
    int PrincipalMoratoriumMonths,
    string RepaymentMethod,
    int PrincipalFrequency,
    int InterestFrequency,
    int DayCountBasis,
    decimal LpcRatePercent,
    string? CreditRating) : IRequest<Guid>, IAuditableAction, ITransactionalCommand;

public sealed class CreateSanctionCommandValidator : AbstractValidator<CreateSanctionCommand>
{
    public CreateSanctionCommandValidator()
    {
        RuleFor(x => x.SanctionId).NotEmpty().MaximumLength(40);
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.ProductCode).NotEmpty();
        RuleFor(x => x.ProjectName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.LoanAmount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.GrantAmount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.InterestRateType).Must(t => t is "Fixed" or "Floating");
        RuleFor(x => x.LoanTenorMonths).GreaterThan(0);
        RuleFor(x => x.NoOfPrincipalRepayments).GreaterThan(0);
        RuleFor(x => x.DayCountBasis).Must(b => b is 360 or 364 or 365);
    }
}

public sealed class CreateSanctionCommandHandler : IRequestHandler<CreateSanctionCommand, Guid>
{
    private readonly ILoanAgreementRepository _repository;
    private readonly ICurrentUserAccessor _currentUser;

    public CreateSanctionCommandHandler(ILoanAgreementRepository repository, ICurrentUserAccessor currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(CreateSanctionCommand request, CancellationToken cancellationToken)
    {
        var existing = await _repository.GetBySanctionIdAsync(request.SanctionId, cancellationToken);
        if (existing is not null)
            throw new InvalidOperationException($"Sanction id '{request.SanctionId}' already exists.");

        var id = Guid.NewGuid();
        var agreement = LoanAgreement.Create(
            id, request.SanctionId, request.CustomerId, request.CustomerNo, request.ProductCode,
            request.ProjectName, request.IndustryType, request.LoanCurrency, request.LoanAmount,
            request.GrantCurrency, request.GrantAmount, request.AgreementDate, request.ExpiryDate,
            request.InterestRateType, request.InitialInterestRatePercent, request.LoanTenorMonths,
            request.NoOfPrincipalRepayments, request.InterestGracePeriodMonths, request.PrincipalMoratoriumMonths,
            request.RepaymentMethod, request.PrincipalFrequency, request.InterestFrequency, request.DayCountBasis,
            request.LpcRatePercent, request.CreditRating, _currentUser.UserId);

        await _repository.AddAsync(agreement, cancellationToken);
        return id;
    }
}
