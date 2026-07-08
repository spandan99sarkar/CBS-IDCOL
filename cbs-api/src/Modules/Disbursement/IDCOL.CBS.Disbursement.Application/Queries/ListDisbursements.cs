using IDCOL.CBS.Disbursement.Application.Abstractions;
using MediatR;

namespace IDCOL.CBS.Disbursement.Application.Queries;

public sealed record DisbursementGlLineDto(string GlCode, string Description, decimal Debit, decimal Credit);

public sealed record DisbursementDto(
    Guid Id, string ReferenceNo, int DisbursementNo, string SanctionRef, string CustomerNo,
    string ProjectName, string LoanCurrency, string Status,
    decimal SuggestedLoanAmount, decimal SuggestedGrantAmount,
    decimal? JustifiedLoanAmount, decimal? JustifiedGrantAmount,
    decimal EffectiveLoanAmount, decimal EffectiveGrantAmount,
    string InitiatedBy, string? ProposedBy, string? PostedBy,
    string? DisbursementMode, DateOnly? ValueDate,
    IReadOnlyList<DisbursementGlLineDto> GlLines);

public sealed record ListDisbursementsQuery : IRequest<IReadOnlyList<DisbursementDto>>;

public sealed class ListDisbursementsQueryHandler
    : IRequestHandler<ListDisbursementsQuery, IReadOnlyList<DisbursementDto>>
{
    private readonly IDisbursementRepository _repository;

    public ListDisbursementsQueryHandler(IDisbursementRepository repository) => _repository = repository;

    public async Task<IReadOnlyList<DisbursementDto>> Handle(
        ListDisbursementsQuery request, CancellationToken cancellationToken)
    {
        var items = await _repository.ListAsync(cancellationToken);
        return items
            .Select(d => new DisbursementDto(
                d.Id, d.ReferenceNo, d.DisbursementNo, d.SanctionRef, d.CustomerNo, d.ProjectName,
                d.LoanCurrency, d.Status, d.SuggestedLoanAmount, d.SuggestedGrantAmount,
                d.JustifiedLoanAmount, d.JustifiedGrantAmount, d.EffectiveLoanAmount, d.EffectiveGrantAmount,
                d.InitiatedBy, d.ProposedBy, d.PostedBy, d.DisbursementMode, d.ValueDate,
                d.GlLines.Select(l => new DisbursementGlLineDto(l.GlCode, l.GlDescription, l.Debit, l.Credit)).ToList()))
            .ToList();
    }
}
