using IDCOL.CBS.CreditSanction.Application.Abstractions;
using IDCOL.CBS.CreditSanction.Domain;
using IDCOL.CBS.LoanLifecycle.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IDCOL.CBS.LoanLifecycle.Infrastructure.Repositories;

public class LoanAgreementRepository : ILoanAgreementRepository
{
    private readonly LoanLifecycleDbContext _db;

    public LoanAgreementRepository(LoanLifecycleDbContext db) => _db = db;

    public Task<LoanAgreement?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _db.LoanAgreements.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    public Task<LoanAgreement?> GetBySanctionIdAsync(string sanctionId, CancellationToken cancellationToken = default) =>
        _db.LoanAgreements.FirstOrDefaultAsync(a => a.SanctionId == sanctionId, cancellationToken);

    public async Task<IReadOnlyList<LoanAgreement>> ListAsync(CancellationToken cancellationToken = default) =>
        await _db.LoanAgreements.OrderByDescending(a => a.CreatedAtUtc).ToListAsync(cancellationToken);

    public async Task AddAsync(LoanAgreement agreement, CancellationToken cancellationToken = default) =>
        await _db.LoanAgreements.AddAsync(agreement, cancellationToken);
}
