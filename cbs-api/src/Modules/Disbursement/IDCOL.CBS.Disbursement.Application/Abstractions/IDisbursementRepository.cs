using IDCOL.CBS.Disbursement.Domain;

namespace IDCOL.CBS.Disbursement.Application.Abstractions;

public interface IDisbursementRepository
{
    Task<DisbursementRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DisbursementRequest>> ListAsync(CancellationToken cancellationToken = default);
    Task<int> CountForSanctionAsync(Guid sanctionId, CancellationToken cancellationToken = default);
    Task AddAsync(DisbursementRequest request, CancellationToken cancellationToken = default);
}
