using IDCOL.CBS.SharedKernel.Domain;

namespace IDCOL.CBS.Classification.Domain;

/// <summary>
/// A single overdue-band rule: a loan of the given finance type / tenor bucket whose period of
/// arrears falls in [MinOverdueMonths, MaxOverdueMonths) carries the given classification status.
/// The whole DFIM 04/2021 threshold matrix lives in these rows (config-driven, versioned by
/// circular and effective date) so a future BB revision is a data change, not a code deploy.
/// </summary>
public class ClassificationThreshold : Entity<Guid>
{
    public string FinanceType { get; private set; } = default!;
    public string? TenorBucket { get; private set; }
    public string Status { get; private set; } = default!;
    public decimal MinOverdueMonths { get; private set; }
    public decimal? MaxOverdueMonths { get; private set; } // null = open-ended (worst band)
    public string CircularRef { get; private set; } = default!;
    public DateOnly EffectiveDate { get; private set; }

    private ClassificationThreshold()
    {
    }

    public static ClassificationThreshold Create(
        Guid id, string financeType, string? tenorBucket, string status,
        decimal minOverdueMonths, decimal? maxOverdueMonths, string circularRef, DateOnly effectiveDate) =>
        new()
        {
            Id = id,
            FinanceType = financeType,
            TenorBucket = tenorBucket,
            Status = status,
            MinOverdueMonths = minOverdueMonths,
            MaxOverdueMonths = maxOverdueMonths,
            CircularRef = circularRef,
            EffectiveDate = effectiveDate,
        };

    public bool Matches(decimal overdueMonths) =>
        overdueMonths >= MinOverdueMonths && (MaxOverdueMonths is null || overdueMonths < MaxOverdueMonths);
}
