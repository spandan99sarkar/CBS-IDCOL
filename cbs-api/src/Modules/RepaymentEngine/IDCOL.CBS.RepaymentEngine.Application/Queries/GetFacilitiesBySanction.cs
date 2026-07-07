using IDCOL.CBS.RepaymentEngine.Application.Abstractions;
using IDCOL.CBS.RepaymentEngine.Domain;
using MediatR;

namespace IDCOL.CBS.RepaymentEngine.Application.Queries;

public sealed record FacilityVersionDto(
    Guid Id, int VersionSequence, string EventType, string Status, DateOnly EffectiveDate, string Label,
    string? SourceFile, decimal? RateBeforePercent, decimal? RateAfterPercent, int? TenorMonthsBefore,
    int? TenorMonthsAfter, decimal CapitalizedAmount, decimal WaivedAmount, decimal OverdueAmountRolledIn,
    string? RegulatoryReference, ScheduleParameters Parameters);

public sealed record FacilityDto(Guid Id, Guid SanctionId, string LenderCode, string Currency, IReadOnlyList<FacilityVersionDto> Versions);

public sealed record GetFacilitiesBySanctionQuery(Guid SanctionId) : IRequest<IReadOnlyList<FacilityDto>>;

public sealed class GetFacilitiesBySanctionQueryHandler
    : IRequestHandler<GetFacilitiesBySanctionQuery, IReadOnlyList<FacilityDto>>
{
    private readonly IFacilityRepository _repository;

    public GetFacilitiesBySanctionQueryHandler(IFacilityRepository repository) => _repository = repository;

    public async Task<IReadOnlyList<FacilityDto>> Handle(
        GetFacilitiesBySanctionQuery request, CancellationToken cancellationToken)
    {
        var facilities = await _repository.ListBySanctionIdAsync(request.SanctionId, cancellationToken);
        return facilities.Select(ToDto).ToList();
    }

    internal static FacilityDto ToDto(Facility f) => new(
        f.Id, f.SanctionId, f.LenderCode, f.Currency,
        f.Versions
            .OrderBy(v => v.VersionSequence)
            .Select(v => new FacilityVersionDto(
                v.Id, v.VersionSequence, v.EventType.ToString(), v.Status.ToString(), v.EffectiveDate, v.Label,
                v.SourceFile, v.RateBeforePercent, v.RateAfterPercent, v.TenorMonthsBefore, v.TenorMonthsAfter,
                v.CapitalizedAmount, v.WaivedAmount, v.OverdueAmountRolledIn, v.RegulatoryReference,
                v.DeserializeParameters()))
            .ToList());
}
