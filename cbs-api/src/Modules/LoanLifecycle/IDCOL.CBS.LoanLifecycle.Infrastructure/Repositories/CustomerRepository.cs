using IDCOL.CBS.LoanLifecycle.Infrastructure.Persistence;
using IDCOL.CBS.PartyKyc.Application.Abstractions;
using IDCOL.CBS.PartyKyc.Domain;
using Microsoft.EntityFrameworkCore;

namespace IDCOL.CBS.LoanLifecycle.Infrastructure.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly LoanLifecycleDbContext _db;

    public CustomerRepository(LoanLifecycleDbContext db) => _db = db;

    public Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _db.Customers.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public Task<Customer?> GetByCustomerNoAsync(string customerNo, CancellationToken cancellationToken = default) =>
        _db.Customers.FirstOrDefaultAsync(c => c.CustomerNo == customerNo, cancellationToken);

    public async Task<IReadOnlyList<Customer>> ListAsync(CancellationToken cancellationToken = default) =>
        await _db.Customers.OrderBy(c => c.CustomerNo).ToListAsync(cancellationToken);

    public async Task AddAsync(Customer customer, CancellationToken cancellationToken = default) =>
        await _db.Customers.AddAsync(customer, cancellationToken);
}
