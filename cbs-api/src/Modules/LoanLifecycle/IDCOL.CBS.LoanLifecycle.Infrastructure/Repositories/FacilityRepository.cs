using IDCOL.CBS.LoanLifecycle.Infrastructure.Persistence;
using IDCOL.CBS.RepaymentEngine.Application.Abstractions;
using IDCOL.CBS.RepaymentEngine.Domain;
using Microsoft.EntityFrameworkCore;

namespace IDCOL.CBS.LoanLifecycle.Infrastructure.Repositories;

public class FacilityRepository : IFacilityRepository
{
    private readonly LoanLifecycleDbContext _db;

    public FacilityRepository(LoanLifecycleDbContext db) => _db = db;

    public Task<Facility?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _db.Facilities.Include(f => f.Versions).FirstOrDefaultAsync(f => f.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Facility>> ListBySanctionIdAsync(Guid sanctionId, CancellationToken cancellationToken = default) =>
        await _db.Facilities.Include(f => f.Versions)
            .Where(f => f.SanctionId == sanctionId)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(Facility facility, CancellationToken cancellationToken = default) =>
        await _db.Facilities.AddAsync(facility, cancellationToken);
}
