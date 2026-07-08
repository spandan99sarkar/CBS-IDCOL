using IDCOL.CBS.Classification.Application.Abstractions;
using MediatR;

namespace IDCOL.CBS.Classification.Application.Queries;

public sealed record ClassificationDto(
    Guid Id, DateOnly AsOfDate, string AccountRef, string CustomerNo, string ProjectName, string Currency,
    string FinanceType, int TenorMonths, string? TenorBucket, decimal Outstanding, decimal OverdueMonths,
    decimal InterestSuspense, decimal EligibleCollateral, string Status, bool IsQualitativeOverride,
    string ProvisionType, decimal ProvisionRatePercent, decimal ProvisionBase, decimal ProvisionRequired);

public sealed record ListClassificationsQuery : IRequest<IReadOnlyList<ClassificationDto>>;

public sealed class ListClassificationsQueryHandler
    : IRequestHandler<ListClassificationsQuery, IReadOnlyList<ClassificationDto>>
{
    private readonly IClassificationRepository _repository;

    public ListClassificationsQueryHandler(IClassificationRepository repository) => _repository = repository;

    public async Task<IReadOnlyList<ClassificationDto>> Handle(
        ListClassificationsQuery request, CancellationToken cancellationToken)
    {
        var rows = await _repository.ListLatestRunAsync(cancellationToken);
        return rows
            .Select(c => new ClassificationDto(
                c.Id, c.AsOfDate, c.AccountRef, c.CustomerNo, c.ProjectName, c.Currency, c.FinanceType,
                c.TenorMonths, c.TenorBucket, c.OutstandingAmount, c.OverdueMonths, c.InterestSuspense,
                c.EligibleCollateral, c.Status, c.IsQualitativeOverride, c.ProvisionType, c.ProvisionRatePercent,
                c.ProvisionBase, c.ProvisionRequired))
            .ToList();
    }
}
