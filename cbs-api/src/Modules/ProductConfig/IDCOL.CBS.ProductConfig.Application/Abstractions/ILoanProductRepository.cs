using IDCOL.CBS.ProductConfig.Domain;

namespace IDCOL.CBS.ProductConfig.Application.Abstractions;

public interface ILoanProductRepository
{
    Task<LoanProduct?> GetByCodeAsync(string productCode, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LoanProduct>> ListAsync(CancellationToken cancellationToken = default);
    Task AddAsync(LoanProduct product, CancellationToken cancellationToken = default);
}
