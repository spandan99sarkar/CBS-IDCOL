using IDCOL.CBS.RepaymentEngine.Domain;

namespace IDCOL.CBS.RepaymentEngine.Application.Abstractions;

public interface IFacilityRepository
{
    Task<Facility?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Facility>> ListBySanctionIdAsync(Guid sanctionId, CancellationToken cancellationToken = default);
    Task AddAsync(Facility facility, CancellationToken cancellationToken = default);
}
