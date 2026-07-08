using IDCOL.CBS.RepaymentEngine.Application.Abstractions;
using IDCOL.CBS.RepaymentEngine.Domain;
using MediatR;

namespace IDCOL.CBS.RepaymentEngine.Application.Queries;

/// <summary>
/// Computes a specific version's schedule on demand from its stored parameters - never persisted
/// row-by-row, so the schedule is always exactly reproducible from (and auditable against) the
/// parameters alone.
/// </summary>
public sealed record GetFacilityVersionScheduleQuery(Guid FacilityId, Guid VersionId) : IRequest<IReadOnlyList<ScheduleRow>>;

public sealed class GetFacilityVersionScheduleQueryHandler
    : IRequestHandler<GetFacilityVersionScheduleQuery, IReadOnlyList<ScheduleRow>>
{
    private readonly IFacilityRepository _repository;

    public GetFacilityVersionScheduleQueryHandler(IFacilityRepository repository) => _repository = repository;

    public async Task<IReadOnlyList<ScheduleRow>> Handle(
        GetFacilityVersionScheduleQuery request, CancellationToken cancellationToken)
    {
        var facility = await _repository.GetByIdAsync(request.FacilityId, cancellationToken)
            ?? throw new InvalidOperationException("Facility not found.");

        var version = facility.Versions.FirstOrDefault(v => v.Id == request.VersionId)
            ?? throw new InvalidOperationException("Facility version not found.");

        return version.ComputeSchedule();
    }
}
