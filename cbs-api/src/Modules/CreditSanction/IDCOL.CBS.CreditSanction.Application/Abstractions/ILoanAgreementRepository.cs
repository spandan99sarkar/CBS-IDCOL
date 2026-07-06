using IDCOL.CBS.CreditSanction.Domain;

namespace IDCOL.CBS.CreditSanction.Application.Abstractions;

public interface ILoanAgreementRepository
{
    Task<LoanAgreement?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<LoanAgreement?> GetBySanctionIdAsync(string sanctionId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LoanAgreement>> ListAsync(CancellationToken cancellationToken = default);
    Task AddAsync(LoanAgreement agreement, CancellationToken cancellationToken = default);
}
