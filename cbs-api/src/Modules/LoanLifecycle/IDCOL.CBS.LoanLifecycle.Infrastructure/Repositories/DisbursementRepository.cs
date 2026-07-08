using IDCOL.CBS.Disbursement.Application.Abstractions;
using IDCOL.CBS.Disbursement.Domain;
using IDCOL.CBS.LoanLifecycle.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IDCOL.CBS.LoanLifecycle.Infrastructure.Repositories;

public class DisbursementRepository : IDisbursementRepository
{
    private readonly LoanLifecycleDbContext _db;

    public DisbursementRepository(LoanLifecycleDbContext db) => _db = db;

    public Task<DisbursementRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _db.DisbursementRequests
            .Include(d => d.GlLines)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

    public async Task<IReadOnlyList<DisbursementRequest>> ListAsync(CancellationToken cancellationToken = default) =>
        await _db.DisbursementRequests
            .Include(d => d.GlLines)
            .OrderByDescending(d => d.InitiatedAtUtc)
            .ToListAsync(cancellationToken);

    public Task<int> CountForSanctionAsync(Guid sanctionId, CancellationToken cancellationToken = default) =>
        _db.DisbursementRequests.CountAsync(d => d.SanctionId == sanctionId, cancellationToken);

    public async Task AddAsync(DisbursementRequest request, CancellationToken cancellationToken = default) =>
        await _db.DisbursementRequests.AddAsync(request, cancellationToken);
}
