using IDCOL.CBS.PartyKyc.Domain;

namespace IDCOL.CBS.PartyKyc.Application.Abstractions;

public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Customer?> GetByCustomerNoAsync(string customerNo, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Customer>> ListAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Customer customer, CancellationToken cancellationToken = default);
}
