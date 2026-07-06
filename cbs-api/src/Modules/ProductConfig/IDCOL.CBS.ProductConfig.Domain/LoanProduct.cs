using IDCOL.CBS.SharedKernel.Domain;

namespace IDCOL.CBS.ProductConfig.Domain;

/// <summary>
/// A configurable loan product. Product-level defaults (rate band, day-count, repayment method,
/// grace rules) seed a Loan Agreement, which may then override them per sanction.
/// </summary>
public class LoanProduct : AggregateRoot<Guid>, IAuditable
{
    public string ProductCode { get; private set; } = default!;
    public string ProductName { get; private set; } = default!;
    public string ProductType { get; private set; } = default!; // Term Loan, Working Capital, Bridge, Lease, etc.
    public string InterestType { get; private set; } = default!; // Fixed | Floating
    public string RepaymentMethod { get; private set; } = default!; // Level Principal | Annuity | PPMT Principal | Scheduled Principal
    public int DayCountBasis { get; private set; } = 360;
    public int GracePeriodMonths { get; private set; }
    public bool PrepaymentAllowed { get; private set; } = true;
    public bool PenaltyAllowed { get; private set; } = true;
    public decimal SuggestedRatePercent { get; private set; }
    public decimal LowerRatePercent { get; private set; }
    public decimal UpperRatePercent { get; private set; }
    public bool IsActive { get; private set; } = true;

    public string CreatedBy { get; private set; } = default!;
    public DateTime CreatedAtUtc { get; private set; }
    public string? LastModifiedBy { get; private set; }
    public DateTime? LastModifiedAtUtc { get; private set; }

    private LoanProduct()
    {
    }

    public static LoanProduct Create(
        Guid id, string productCode, string productName, string productType, string interestType,
        string repaymentMethod, int dayCountBasis, int gracePeriodMonths, bool prepaymentAllowed,
        bool penaltyAllowed, decimal suggestedRatePercent, decimal lowerRatePercent, decimal upperRatePercent,
        string createdBy)
    {
        if (string.IsNullOrWhiteSpace(productCode)) throw new ArgumentException("Product code is required.", nameof(productCode));
        if (string.IsNullOrWhiteSpace(productName)) throw new ArgumentException("Product name is required.", nameof(productName));
        if (lowerRatePercent > upperRatePercent)
            throw new ArgumentException("Lower rate cannot exceed upper rate.");

        return new LoanProduct
        {
            Id = id,
            ProductCode = productCode.Trim().ToUpperInvariant(),
            ProductName = productName.Trim(),
            ProductType = productType,
            InterestType = interestType,
            RepaymentMethod = repaymentMethod,
            DayCountBasis = dayCountBasis,
            GracePeriodMonths = gracePeriodMonths,
            PrepaymentAllowed = prepaymentAllowed,
            PenaltyAllowed = penaltyAllowed,
            SuggestedRatePercent = suggestedRatePercent,
            LowerRatePercent = lowerRatePercent,
            UpperRatePercent = upperRatePercent,
            IsActive = true,
            CreatedBy = createdBy,
            CreatedAtUtc = DateTime.UtcNow,
        };
    }

    public void Deactivate(string modifiedBy)
    {
        IsActive = false;
        Touch(modifiedBy);
    }

    private void Touch(string modifiedBy)
    {
        LastModifiedBy = modifiedBy;
        LastModifiedAtUtc = DateTime.UtcNow;
    }
}
