using IDCOL.CBS.CreditSanction.Application.Abstractions;
using MediatR;

namespace IDCOL.CBS.CreditSanction.Application.Sanctions;

public sealed record SanctionListItemDto(
    Guid Id, string SanctionId, string CustomerNo, string ProductCode, string ProjectName,
    string LoanCurrency, decimal LoanAmount, decimal GrantAmount, DateOnly AgreementDate,
    int NoOfPrincipalRepayments, string Status);

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
                a.NoOfPrincipalRepayments, a.Status))
            .ToList();
    }
}
