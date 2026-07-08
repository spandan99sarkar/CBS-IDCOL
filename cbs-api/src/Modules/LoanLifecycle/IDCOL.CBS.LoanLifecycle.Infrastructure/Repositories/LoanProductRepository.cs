using IDCOL.CBS.LoanLifecycle.Infrastructure.Persistence;
using IDCOL.CBS.ProductConfig.Application.Abstractions;
using IDCOL.CBS.ProductConfig.Domain;
using Microsoft.EntityFrameworkCore;

namespace IDCOL.CBS.LoanLifecycle.Infrastructure.Repositories;

public class LoanProductRepository : ILoanProductRepository
{
    private readonly LoanLifecycleDbContext _db;

    public LoanProductRepository(LoanLifecycleDbContext db) => _db = db;

    public Task<LoanProduct?> GetByCodeAsync(string productCode, CancellationToken cancellationToken = default) =>
        _db.LoanProducts.FirstOrDefaultAsync(p => p.ProductCode == productCode.ToUpper(), cancellationToken);

    public async Task<IReadOnlyList<LoanProduct>> ListAsync(CancellationToken cancellationToken = default) =>
        await _db.LoanProducts.OrderBy(p => p.ProductCode).ToListAsync(cancellationToken);

    public async Task AddAsync(LoanProduct product, CancellationToken cancellationToken = default) =>
        await _db.LoanProducts.AddAsync(product, cancellationToken);
}
