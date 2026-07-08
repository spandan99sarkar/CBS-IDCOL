using IDCOL.CBS.CreditSanction.Application.Abstractions;
using MediatR;

namespace IDCOL.CBS.CreditSanction.Application.Sanctions;

public sealed record SanctionListItemDto(
    Guid Id, string SanctionId, string CustomerNo, string ProductCode, string ProjectName,
    string LoanCurrency, decimal LoanAmount, decimal GrantAmount, DateOnly AgreementDate,
    int NoOfPrincipalRepayments, int LoanTenorMonths, string Status,
    decimal InitialInterestRatePercent, string RepaymentMethod, int PrincipalFrequency,
    int DayCountBasis, int InterestGracePeriodMonths, int PrincipalMoratoriumMonths);

public sealed record ListSanctionsQuery : IRequest<IReadOnlyList<SanctionListItemDto>>;

public sealed class ListSanctionsQueryHandler
    : IRequestHandler<ListSanctionsQuery, IReadOnlyList<SanctionListItemDto>>
{
    private readonly ILoanAgreementRepository _repository;

    public ListSanctionsQueryHandler(ILoanAgreementRepository repository) => _repository = repository;

    public async Task<IReadOnlyList<SanctionListItemDto>> Handle(
        ListSanctionsQuery request, CancellationToken cancellationToken)
    {
        var agreements = await _repository.ListAsync(cancellationToken);
        return agreements
            .Select(a => new SanctionListItemDto(
                a.Id, a.SanctionId, a.CustomerNo, a.ProductCode, a.ProjectName,
                a.LoanCurrency, a.LoanAmount, a.GrantAmount, a.AgreementDate,
                a.NoOfPrincipalRepayments, a.LoanTenorMonths, a.Status,
                a.InitialInterestRatePercent, a.RepaymentMethod, a.PrincipalFrequency,
                a.DayCountBasis, a.InterestGracePeriodMonths, a.PrincipalMoratoriumMonths))
            .ToList();
    }
}
