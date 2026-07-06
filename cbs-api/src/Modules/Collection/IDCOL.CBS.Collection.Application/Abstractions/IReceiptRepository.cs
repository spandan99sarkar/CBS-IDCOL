using IDCOL.CBS.Collection.Domain;

namespace IDCOL.CBS.Collection.Application.Abstractions;

public interface IReceiptRepository
{
    Task<Receipt?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Receipt>> ListAsync(CancellationToken cancellationToken = default);
    Task<int> CountAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Receipt receipt, CancellationToken cancellationToken = default);
}
