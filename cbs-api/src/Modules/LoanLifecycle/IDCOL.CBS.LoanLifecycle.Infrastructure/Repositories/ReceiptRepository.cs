using IDCOL.CBS.Collection.Application.Abstractions;
using IDCOL.CBS.Collection.Domain;
using IDCOL.CBS.LoanLifecycle.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IDCOL.CBS.LoanLifecycle.Infrastructure.Repositories;

public class ReceiptRepository : IReceiptRepository
{
    private readonly LoanLifecycleDbContext _db;

    public ReceiptRepository(LoanLifecycleDbContext db) => _db = db;

    public Task<Receipt?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _db.Receipts.Include(r => r.GlLines).FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Receipt>> ListAsync(CancellationToken cancellationToken = default) =>
        await _db.Receipts.Include(r => r.GlLines).OrderByDescending(r => r.EnteredAtUtc).ToListAsync(cancellationToken);

    public Task<int> CountAsync(CancellationToken cancellationToken = default) =>
        _db.Receipts.CountAsync(cancellationToken);

    public async Task AddAsync(Receipt receipt, CancellationToken cancellationToken = default) =>
        await _db.Receipts.AddAsync(receipt, cancellationToken);
}
