using IDCOL.CBS.SharedKernel.Domain;

namespace IDCOL.CBS.Classification.Domain;

/// <summary>
/// The result of classifying one loan account at a point in time (a row of the classification
/// history / CL statement). Captures the inputs, the resulting status, and the required provision
/// so a run is fully reproducible and auditable for BB inspection.
/// </summary>
public class LoanClassification : AggregateRoot<Guid>, IAuditable
{
    public Guid RunId { get; private set; }
    public DateOnly AsOfDate { get; private set; }

    public Guid AccountId { get; private set; }
    public string AccountRef { get; private set; } = default!;
    public string CustomerNo { get; private set; } = default!;
    public string ProjectName { get; private set; } = default!;
    public string Currency { get; private set; } = "BDT";

    public string FinanceType { get; private set; } = default!;
    public int TenorMonths { get; private set; }
    public string? TenorBucket { get; private set; }
    public bool IsCmsme { get; private set; }

    public decimal OutstandingAmount { get; private set; }
    public decimal OverdueMonths { get; private set; }
    public decimal InterestSuspense { get; private set; }
    public decimal EligibleCollateral { get; private set; }

    public string Status { get; private set; } = default!;
    public bool IsQualitativeOverride { get; private set; }
    public string? QualitativeReason { get; private set; }

    public string ProvisionType { get; private set; } = default!;
    public decimal ProvisionRatePercent { get; private set; }
    public decimal ProvisionBase { get; private set; }
    public decimal ProvisionRequired { get; private set; }

    public string CreatedBy { get; private set; } = default!;
    public DateTime CreatedAtUtc { get; private set; }
    public string? LastModifiedBy { get; private set; }
    public DateTime? LastModifiedAtUtc { get; private set; }

    private LoanClassification()
    {
    }

    public static LoanClassification Create(
        Guid id, Guid runId, DateOnly asOfDate, Guid accountId, string accountRef, string customerNo,
        string projectName, string currency, string financeType, int tenorMonths, string? tenorBucket,
        bool isCmsme, decimal outstanding, decimal overdueMonths, decimal interestSuspense,
        decimal eligibleCollateral, string status, bool isQualitativeOverride, string? qualitativeReason,
        AccountClassificationResult result, string createdBy) =>
        new()
        {
            Id = id,
            RunId = runId,
            AsOfDate = asOfDate,
            AccountId = accountId,
            AccountRef = accountRef,
            CustomerNo = customerNo,
            ProjectName = projectName,
            Currency = currency,
            FinanceType = financeType,
            TenorMonths = tenorMonths,
            TenorBucket = tenorBucket,
            IsCmsme = isCmsme,
            OutstandingAmount = outstanding,
            OverdueMonths = overdueMonths,
            InterestSuspense = interestSuspense,
            EligibleCollateral = eligibleCollateral,
            Status = status,
            IsQualitativeOverride = isQualitativeOverride,
            QualitativeReason = qualitativeReason,
            ProvisionType = result.ProvisionType,
            ProvisionRatePercent = result.ProvisionRatePercent,
            ProvisionBase = result.ProvisionBase,
            ProvisionRequired = result.ProvisionRequired,
            CreatedBy = createdBy,
            CreatedAtUtc = DateTime.UtcNow,
        };
}
