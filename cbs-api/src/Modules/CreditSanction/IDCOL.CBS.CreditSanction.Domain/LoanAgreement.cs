using IDCOL.CBS.SharedKernel.Common;
using IDCOL.CBS.SharedKernel.Domain;

namespace IDCOL.CBS.CreditSanction.Domain;

/// <summary>
/// A loan sanction / agreement: the approved terms for a borrower before any money moves.
/// References a Customer and a LoanProduct, and carries the repayment terms that later drive the
/// repayment engine and the disbursement workflow. Loan and Grant amounts are tracked separately,
/// as IDCOL sanctions routinely combine a loan facility with a grant.
/// </summary>
public class LoanAgreement : AggregateRoot<Guid>, IAuditable
{
    public string SanctionId { get; private set; } = default!;
    public int Version { get; private set; } = 1;

    public Guid CustomerId { get; private set; }
    public string CustomerNo { get; private set; } = default!;
    public string ProductCode { get; private set; } = default!;
    public string ProjectName { get; private set; } = default!;
    public string? IndustryType { get; private set; }

    public string LoanCurrency { get; private set; } = "BDT";
    public decimal LoanAmount { get; private set; }
    public string GrantCurrency { get; private set; } = "BDT";
    public decimal GrantAmount { get; private set; }

    public DateOnly AgreementDate { get; private set; }
    public DateOnly? ExpiryDate { get; private set; }

    public string InterestRateType { get; private set; } = "Fixed"; // Fixed | Floating
    public decimal InitialInterestRatePercent { get; private set; }
    public int LoanTenorMonths { get; private set; }
    public int NoOfPrincipalRepayments { get; private set; }
    public int InterestGracePeriodMonths { get; private set; }
    public int PrincipalMoratoriumMonths { get; private set; }
    public string RepaymentMethod { get; private set; } = "Level Principal";
    public int PrincipalFrequency { get; private set; } = 4; // installments per year
    public int InterestFrequency { get; private set; } = 4;
    public int DayCountBasis { get; private set; } = 360;
    public decimal LpcRatePercent { get; private set; }
    public string? CreditRating { get; private set; }

    public string Status { get; private set; } = "Draft"; // Draft | Signed | Active | Closed

    public string CreatedBy { get; private set; } = default!;
    public DateTime CreatedAtUtc { get; private set; }
    public string? LastModifiedBy { get; private set; }
    public DateTime? LastModifiedAtUtc { get; private set; }

    private LoanAgreement()
    {
    }

    public static LoanAgreement Create(
        Guid id, string sanctionId, Guid customerId, string customerNo, string productCode,
        string projectName, string? industryType, string loanCurrency, decimal loanAmount,
        string grantCurrency, decimal grantAmount, DateOnly agreementDate, DateOnly? expiryDate,
        string interestRateType, decimal initialInterestRatePercent, int loanTenorMonths,
        int noOfPrincipalRepayments, int interestGracePeriodMonths, int principalMoratoriumMonths,
        string repaymentMethod, int principalFrequency, int interestFrequency, int dayCountBasis,
        decimal lpcRatePercent, string? creditRating, string createdBy)
    {
        if (string.IsNullOrWhiteSpace(sanctionId)) throw new ArgumentException("Sanction id is required.", nameof(sanctionId));
        if (string.IsNullOrWhiteSpace(projectName)) throw new ArgumentException("Project name is required.", nameof(projectName));
        if (loanAmount < 0 || grantAmount < 0) throw new ArgumentException("Amounts cannot be negative.");

        return new LoanAgreement
        {
            Id = id,
            SanctionId = sanctionId.Trim(),
            Version = 1,
            CustomerId = customerId,
            CustomerNo = customerNo,
            ProductCode = productCode,
            ProjectName = projectName.Trim(),
            IndustryType = industryType,
            LoanCurrency = loanCurrency,
            LoanAmount = loanAmount,
            GrantCurrency = grantCurrency,
            GrantAmount = grantAmount,
            AgreementDate = agreementDate,
            ExpiryDate = expiryDate,
            InterestRateType = interestRateType,
            InitialInterestRatePercent = initialInterestRatePercent,
            LoanTenorMonths = loanTenorMonths,
            NoOfPrincipalRepayments = noOfPrincipalRepayments,
            InterestGracePeriodMonths = interestGracePeriodMonths,
            PrincipalMoratoriumMonths = principalMoratoriumMonths,
            RepaymentMethod = repaymentMethod,
            PrincipalFrequency = principalFrequency == 0 ? 4 : principalFrequency,
            InterestFrequency = interestFrequency == 0 ? 4 : interestFrequency,
            DayCountBasis = dayCountBasis == 0 ? 360 : dayCountBasis,
            LpcRatePercent = lpcRatePercent,
            CreditRating = creditRating,
            Status = "Draft",
            CreatedBy = createdBy,
            CreatedAtUtc = DateTime.UtcNow,
        };
    }

    /// <summary>Moves a Draft sanction to Signed - the precondition for account opening and disbursement.</summary>
    public Result Sign(string modifiedBy)
    {
        if (Status != "Draft")
            return Result.Fail($"Only a Draft sanction can be signed (current status: {Status}).");

        Status = "Signed";
        LastModifiedBy = modifiedBy;
        LastModifiedAtUtc = DateTime.UtcNow;
        return Result.Success();
    }
}
