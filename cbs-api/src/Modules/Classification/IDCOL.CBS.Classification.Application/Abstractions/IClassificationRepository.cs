using IDCOL.CBS.Classification.Domain;

namespace IDCOL.CBS.Classification.Application.Abstractions;

public interface IClassificationRepository
{
    Task<IReadOnlyList<ClassificationThreshold>> GetThresholdsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProvisioningRate>> GetRatesAsync(CancellationToken cancellationToken = default);
    Task AddAsync(LoanClassification classification, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LoanClassification>> ListLatestRunAsync(CancellationToken cancellationToken = default);
}
