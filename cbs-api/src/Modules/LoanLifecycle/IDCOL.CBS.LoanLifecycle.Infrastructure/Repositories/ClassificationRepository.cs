using IDCOL.CBS.Classification.Application.Abstractions;
using IDCOL.CBS.Classification.Domain;
using IDCOL.CBS.LoanLifecycle.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IDCOL.CBS.LoanLifecycle.Infrastructure.Repositories;

public class ClassificationRepository : IClassificationRepository
{
    private readonly LoanLifecycleDbContext _db;

    public ClassificationRepository(LoanLifecycleDbContext db) => _db = db;

    public async Task<IReadOnlyList<ClassificationThreshold>> GetThresholdsAsync(CancellationToken cancellationToken = default) =>
        await _db.ClassificationThresholds.AsNoTracking().ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<ProvisioningRate>> GetRatesAsync(CancellationToken cancellationToken = default) =>
        await _db.ProvisioningRates.AsNoTracking().ToListAsync(cancellationToken);

    public async Task AddAsync(LoanClassification classification, CancellationToken cancellationToken = default) =>
        await _db.LoanClassifications.AddAsync(classification, cancellationToken);

    public async Task<IReadOnlyList<LoanClassification>> ListLatestRunAsync(CancellationToken cancellationToken = default)
    {
        var latest = await _db.LoanClassifications
            .OrderByDescending(c => c.CreatedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);
        if (latest is null) return new List<LoanClassification>();

        return await _db.LoanClassifications
            .Where(c => c.RunId == latest.RunId)
            .OrderBy(c => c.ProjectName)
            .ToListAsync(cancellationToken);
    }
}
