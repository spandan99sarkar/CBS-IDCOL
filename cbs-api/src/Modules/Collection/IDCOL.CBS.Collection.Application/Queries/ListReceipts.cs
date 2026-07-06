using IDCOL.CBS.Collection.Application.Abstractions;
using MediatR;

namespace IDCOL.CBS.Collection.Application.Queries;

public sealed record ReceiptGlLineDto(string GlCode, string Description, decimal Debit, decimal Credit);

public sealed record ReceiptDto(
    Guid Id, string ReferenceNo, string SanctionRef, string CustomerNo, string ProjectName, string Currency,
    string PaymentMode, string? InstrumentNo, decimal InstrumentAmount, DateOnly ValueDate, DateOnly ReceiveDate,
    decimal PrincipalAmount, decimal InterestAmount, decimal LpcAmount, string Status,
    string EnteredBy, string? VerifiedBy, IReadOnlyList<ReceiptGlLineDto> GlLines);

public sealed record ListReceiptsQuery : IRequest<IReadOnlyList<ReceiptDto>>;

public sealed class ListReceiptsQueryHandler : IRequestHandler<ListReceiptsQuery, IReadOnlyList<ReceiptDto>>
{
    private readonly IReceiptRepository _repository;

    public ListReceiptsQueryHandler(IReceiptRepository repository) => _repository = repository;

    public async Task<IReadOnlyList<ReceiptDto>> Handle(ListReceiptsQuery request, CancellationToken cancellationToken)
    {
        var receipts = await _repository.ListAsync(cancellationToken);
        return receipts
            .Select(r => new ReceiptDto(
                r.Id, r.ReferenceNo, r.SanctionRef, r.CustomerNo, r.ProjectName, r.Currency, r.PaymentMode,
                r.InstrumentNo, r.InstrumentAmount, r.ValueDate, r.ReceiveDate, r.PrincipalAmount, r.InterestAmount,
                r.LpcAmount, r.Status, r.EnteredBy, r.VerifiedBy,
                r.GlLines.Select(l => new ReceiptGlLineDto(l.GlCode, l.GlDescription, l.Debit, l.Credit)).ToList()))
            .ToList();
    }
}
