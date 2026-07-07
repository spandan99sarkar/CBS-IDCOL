using IDCOL.CBS.SharedKernel.Common;
using IDCOL.CBS.SharedKernel.Domain;

namespace IDCOL.CBS.RepaymentEngine.Domain;

/// <summary>
/// A lender-tranche of a sanctioned loan. Almost always one Facility per Sanction, but a
/// co-financed project (e.g. an IDCOL tranche alongside a Trust Bank tranche on the same project)
/// is modeled as two Facilities sharing a SanctionId, each with its own independent version chain -
/// validated against BPCL's and MAGBL's real multi-lender portfolios.
/// </summary>
public class Facility : AggregateRoot<Guid>, IAuditable
{
    public Guid SanctionId { get; private set; }
    public string LenderCode { get; private set; } = default!; // "IDCOL", "TRUST_BANK", "BIFFL", ...
    public string Currency { get; private set; } = "BDT";

    private readonly List<FacilityVersion> _versions = new();
    public IReadOnlyCollection<FacilityVersion> Versions => _versions.AsReadOnly();

    public FacilityVersion CurrentVersion =>
        _versions.OrderByDescending(v => v.VersionSequence).First(v => v.Status == FacilityVersionStatus.Active);

    public string CreatedBy { get; private set; } = default!;
    public DateTime CreatedAtUtc { get; private set; }
    public string? LastModifiedBy { get; private set; }
    public DateTime? LastModifiedAtUtc { get; private set; }

    private Facility()
    {
    }

    /// <summary>Creates a Facility with its ORIGINAL (version 0) schedule, as sanctioned.</summary>
    public static Facility CreateOriginal(
        Guid id, Guid sanctionId, string lenderCode, string currency, DateOnly effectiveDate,
        string parametersJson, string createdBy)
    {
        if (string.IsNullOrWhiteSpace(lenderCode))
            throw new ArgumentException("Lender code is required.", nameof(lenderCode));

        var facility = new Facility
        {
            Id = id,
            SanctionId = sanctionId,
            LenderCode = lenderCode.Trim().ToUpperInvariant(),
            Currency = currency,
            CreatedBy = createdBy,
            CreatedAtUtc = DateTime.UtcNow,
        };

        var original = FacilityVersion.Create(
            Guid.NewGuid(), facility.Id, 0, FacilityVersionEventType.Original, effectiveDate,
            "Original (as sanctioned)", null, null, null, null, null, 0, 0, 0, null,
            parametersJson, createdBy);

        facility._versions.Add(original);
        return facility;
    }

    /// <summary>
    /// Adds a new version on top of the current one (reschedule/restructure/rate-change/prepayment/
    /// moratorium-extension), superseding it. The delta fields are provenance, not recomputed - see
    /// the architecture plan's rationale for keeping them first-class rather than inferred later.
    /// </summary>
    public Result<FacilityVersion> AddVersion(
        Guid newVersionId, FacilityVersionEventType eventType, DateOnly effectiveDate, string label,
        string? sourceFile, decimal? rateBeforePercent, decimal? rateAfterPercent, int? tenorMonthsBefore,
        int? tenorMonthsAfter, decimal capitalizedAmount, decimal waivedAmount, decimal overdueAmountRolledIn,
        string? regulatoryReference, string parametersJson, string createdBy)
    {
        var current = CurrentVersion;
        if (effectiveDate < current.EffectiveDate)
            return Result.Fail<FacilityVersion>(
                $"New version's effective date ({effectiveDate}) cannot precede the current version's ({current.EffectiveDate}).");

        var newVersion = FacilityVersion.Create(
            newVersionId, Id, current.VersionSequence + 1, eventType, effectiveDate, label, sourceFile,
            rateBeforePercent, rateAfterPercent, tenorMonthsBefore, tenorMonthsAfter, capitalizedAmount,
            waivedAmount, overdueAmountRolledIn, regulatoryReference, parametersJson, createdBy);

        current.Supersede(createdBy);
        _versions.Add(newVersion);
        LastModifiedBy = createdBy;
        LastModifiedAtUtc = DateTime.UtcNow;
        return Result.Success(newVersion);
    }
}
