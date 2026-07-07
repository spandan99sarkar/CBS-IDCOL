using System.Text.Json;
using IDCOL.CBS.SharedKernel.Domain;

namespace IDCOL.CBS.RepaymentEngine.Domain;

/// <summary>
/// One immutable "generation" of a Facility's schedule - the original sanction, or a later
/// reschedule/restructure/rate-change/prepayment/moratorium-extension. Stores the full parameter
/// set needed to regenerate its schedule via <see cref="RepaymentScheduleEngine"/> (never the rows
/// themselves - they're deterministic from the params, recomputed on every read, which is what
/// makes the engine auditable: the same inputs always reproduce the same schedule).
///
/// The delta fields (rate/tenor before-after, capitalized/waived/overdue-rolled-in amounts,
/// regulatory reference) are first-class, not inferred from a diff - validated necessary against
/// real reschedule history across the portfolio (e.g. PABL's 4th reschedule rolls a specific
/// overdue-principal figure into the new opening balance; that provenance would be lost if only
/// the resulting schedule were kept).
/// </summary>
public class FacilityVersion : Entity<Guid>, IAuditable
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = false };

    public Guid FacilityId { get; private set; }
    public int VersionSequence { get; private set; }
    public FacilityVersionEventType EventType { get; private set; }
    public FacilityVersionStatus Status { get; private set; } = FacilityVersionStatus.Active;
    public DateOnly EffectiveDate { get; private set; }
    public string Label { get; private set; } = default!;
    public string? SourceFile { get; private set; }

    public decimal? RateBeforePercent { get; private set; }
    public decimal? RateAfterPercent { get; private set; }
    public int? TenorMonthsBefore { get; private set; }
    public int? TenorMonthsAfter { get; private set; }
    public decimal CapitalizedAmount { get; private set; }
    public decimal WaivedAmount { get; private set; }
    public decimal OverdueAmountRolledIn { get; private set; }
    public string? RegulatoryReference { get; private set; }

    /// <summary>Serialized <see cref="ScheduleParameters"/> - the sole source of truth for this version's schedule.</summary>
    public string ParametersJson { get; private set; } = default!;

    public string CreatedBy { get; private set; } = default!;
    public DateTime CreatedAtUtc { get; private set; }
    public string? LastModifiedBy { get; private set; }
    public DateTime? LastModifiedAtUtc { get; private set; }

    private FacilityVersion()
    {
    }

    internal static FacilityVersion Create(
        Guid id, Guid facilityId, int versionSequence, FacilityVersionEventType eventType, DateOnly effectiveDate,
        string label, string? sourceFile, decimal? rateBeforePercent, decimal? rateAfterPercent,
        int? tenorMonthsBefore, int? tenorMonthsAfter, decimal capitalizedAmount, decimal waivedAmount,
        decimal overdueAmountRolledIn, string? regulatoryReference, string parametersJson, string createdBy) =>
        new()
        {
            Id = id,
            FacilityId = facilityId,
            VersionSequence = versionSequence,
            EventType = eventType,
            Status = FacilityVersionStatus.Active,
            EffectiveDate = effectiveDate,
            Label = label,
            SourceFile = sourceFile,
            RateBeforePercent = rateBeforePercent,
            RateAfterPercent = rateAfterPercent,
            TenorMonthsBefore = tenorMonthsBefore,
            TenorMonthsAfter = tenorMonthsAfter,
            CapitalizedAmount = capitalizedAmount,
            WaivedAmount = waivedAmount,
            OverdueAmountRolledIn = overdueAmountRolledIn,
            RegulatoryReference = regulatoryReference,
            ParametersJson = parametersJson,
            CreatedBy = createdBy,
            CreatedAtUtc = DateTime.UtcNow,
        };

    internal void Supersede(string modifiedBy)
    {
        Status = FacilityVersionStatus.Superseded;
        LastModifiedBy = modifiedBy;
        LastModifiedAtUtc = DateTime.UtcNow;
    }

    public ScheduleParameters DeserializeParameters() =>
        JsonSerializer.Deserialize<ScheduleParameters>(ParametersJson)
        ?? throw new InvalidOperationException($"FacilityVersion {Id} has corrupt parameters JSON.");

    public List<ScheduleRow> ComputeSchedule() => RepaymentScheduleEngine.Generate(DeserializeParameters());

    /// <summary>
    /// In-table schedule modification: pins a specific installment's interest/opening/closing
    /// balance to an explicit value (the same override mechanism already used for the 5 real
    /// borrowers whose historical schedules can't be reproduced from first principles). Applying
    /// an override is itself an auditable change to this version - callers are expected to record
    /// it via the normal maker-checker/audit pipeline at the application layer.
    /// </summary>
    public void ApplyInstallmentOverride(
        int installmentIndex, decimal? interestOverride, decimal? openingBalanceOverride,
        decimal? closingBalanceOverride, string modifiedBy)
    {
        var parameters = DeserializeParameters();
        if (installmentIndex < 0 || installmentIndex >= parameters.NumInstallments)
            throw new ArgumentOutOfRangeException(nameof(installmentIndex),
                $"Installment index must be within [0, {parameters.NumInstallments}).");

        // The engine treats a non-null override array as authoritative for EVERY row, not just the
        // one being edited (see RepaymentScheduleEngine.Generate: "if (intOverride != null) ..."
        // applies intOverride[i] to every i). So the first time any override is applied to this
        // version, every array must be backfilled with the CURRENTLY-COMPUTED value per row -
        // otherwise every non-edited installment would silently collapse to 0 once one row is
        // overridden. Only materialize the arrays that don't already exist.
        if (parameters.InterestPaymentAmounts is null
            || parameters.OpeningBalanceAmounts is null
            || parameters.ClosingBalanceAmounts is null)
        {
            var naturalRows = RepaymentScheduleEngine.Generate(parameters);
            // Interest override sets CashInterest on a normal row or CapInterest on a capitalization
            // row (never the combined display total) - see the engine's isCapRow branch.
            parameters.InterestPaymentAmounts ??= naturalRows
                .Select(r => r.IsCapRow ? r.CapInterest : r.CashInterest).ToArray();
            parameters.OpeningBalanceAmounts ??= naturalRows.Select(r => r.OpeningBal).ToArray();
            parameters.ClosingBalanceAmounts ??= naturalRows.Select(r => r.ClosingBal).ToArray();
        }

        if (interestOverride.HasValue) parameters.InterestPaymentAmounts![installmentIndex] = (double)interestOverride.Value;
        if (openingBalanceOverride.HasValue) parameters.OpeningBalanceAmounts![installmentIndex] = (double)openingBalanceOverride.Value;
        if (closingBalanceOverride.HasValue) parameters.ClosingBalanceAmounts![installmentIndex] = (double)closingBalanceOverride.Value;

        ParametersJson = JsonSerializer.Serialize(parameters, JsonOptions);
        LastModifiedBy = modifiedBy;
        LastModifiedAtUtc = DateTime.UtcNow;
    }
}
