using IDCOL.CBS.SharedKernel.Domain;

namespace IDCOL.CBS.Classification.Domain;

/// <summary>
/// The provision percentage for a classification status (config-driven per DFIM 04/2021).
/// General provision applies to Standard/SMA; specific provision to the classified statuses.
/// Standard has two rates (CMSME vs other) distinguished by <see cref="IsCmsme"/>.
/// </summary>
public class ProvisioningRate : Entity<Guid>
{
    public string Status { get; private set; } = default!;
    public bool IsCmsme { get; private set; }
    public string ProvisionType { get; private set; } = default!; // General | Specific
    public decimal RatePercent { get; private set; }
    public string CircularRef { get; private set; } = default!;
    public DateOnly EffectiveDate { get; private set; }

    private ProvisioningRate()
    {
    }

    public static ProvisioningRate Create(
        Guid id, string status, bool isCmsme, string provisionType, decimal ratePercent,
        string circularRef, DateOnly effectiveDate) =>
        new()
        {
            Id = id,
            Status = status,
            IsCmsme = isCmsme,
            ProvisionType = provisionType,
            RatePercent = ratePercent,
            CircularRef = circularRef,
            EffectiveDate = effectiveDate,
        };
}
