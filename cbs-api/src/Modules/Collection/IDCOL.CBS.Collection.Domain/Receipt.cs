using IDCOL.CBS.SharedKernel.Common;
using IDCOL.CBS.SharedKernel.Domain;

namespace IDCOL.CBS.Collection.Domain;

/// <summary>
/// A repayment received against a loan, moving through IDCOL's 2-stage collection control:
/// Pending (CAD records the receipt and its Principal/Interest/LPC breakdown) -> Verified
/// (Accounts reconciles and posts to GL). The GL impact only occurs on verification, and the
/// verifier cannot be the same user who entered the receipt (structural maker-checker).
/// </summary>
public class Receipt : AggregateRoot<Guid>, IAuditable
{
    public string ReferenceNo { get; private set; } = default!;

    public Guid SanctionId { get; private set; }
    public string SanctionRef { get; private set; } = default!;
    public string CustomerNo { get; private set; } = default!;
    public string ProjectName { get; private set; } = default!;
    public string Currency { get; private set; } = "BDT";

    public string PaymentMode { get; private set; } = default!; // Cash | Cheque | PayOrder | EFT | RTGS | SWIFT | PDC
    public string? InstrumentNo { get; private set; }
    public string? BankName { get; private set; }
    public decimal InstrumentAmount { get; private set; }

    public DateOnly ValueDate { get; private set; }
    public DateOnly ReceiveDate { get; private set; }
    public DateOnly? LpcDate { get; private set; }

    // Breakdown - CAD allocates the received amount across the three buckets (sums to InstrumentAmount).
    public decimal PrincipalAmount { get; private set; }
    public decimal InterestAmount { get; private set; }
    public decimal LpcAmount { get; private set; }

    public string Status { get; private set; } = "Pending"; // Pending | Verified

    public string EnteredBy { get; private set; } = default!;
    public DateTime EnteredAtUtc { get; private set; }
    public string? VerifiedBy { get; private set; }
    public DateTime? VerifiedAtUtc { get; private set; }
    public string? VerifyComment { get; private set; }

    private readonly List<ReceiptGlLine> _glLines = new();
    public IReadOnlyCollection<ReceiptGlLine> GlLines => _glLines.AsReadOnly();

    public string CreatedBy { get; private set; } = default!;
    public DateTime CreatedAtUtc { get; private set; }
    public string? LastModifiedBy { get; private set; }
    public DateTime? LastModifiedAtUtc { get; private set; }

    private Receipt()
    {
    }

    /// <summary>Stage 1 (CAD): record a received payment and its allocation.</summary>
    public static Receipt Enter(
        Guid id, string referenceNo, Guid sanctionId, string sanctionRef, string customerNo, string projectName,
        string currency, string paymentMode, string? instrumentNo, string? bankName, decimal instrumentAmount,
        DateOnly valueDate, DateOnly receiveDate, DateOnly? lpcDate, decimal principalAmount, decimal interestAmount,
        decimal lpcAmount, string enteringUserId)
    {
        if (string.IsNullOrWhiteSpace(referenceNo)) throw new ArgumentException("Reference no is required.", nameof(referenceNo));
        if (instrumentAmount <= 0) throw new ArgumentException("Instrument amount must be positive.");
        if (principalAmount < 0 || interestAmount < 0 || lpcAmount < 0) throw new ArgumentException("Allocation amounts cannot be negative.");
        if (Math.Round(principalAmount + interestAmount + lpcAmount - instrumentAmount, 2) != 0)
            throw new ArgumentException($"Allocation ({principalAmount + interestAmount + lpcAmount:N2}) must equal the instrument amount ({instrumentAmount:N2}).");

        return new Receipt
        {
            Id = id,
            ReferenceNo = referenceNo,
            SanctionId = sanctionId,
            SanctionRef = sanctionRef,
            CustomerNo = customerNo,
            ProjectName = projectName,
            Currency = currency,
            PaymentMode = paymentMode,
            InstrumentNo = instrumentNo,
            BankName = bankName,
            InstrumentAmount = instrumentAmount,
            ValueDate = valueDate,
            ReceiveDate = receiveDate,
            LpcDate = lpcDate,
            PrincipalAmount = principalAmount,
            InterestAmount = interestAmount,
            LpcAmount = lpcAmount,
            Status = "Pending",
            EnteredBy = enteringUserId,
            EnteredAtUtc = DateTime.UtcNow,
            CreatedBy = enteringUserId,
            CreatedAtUtc = DateTime.UtcNow,
        };
    }

    /// <summary>Stage 2 (Accounts): reconcile and post the receipt to the GL.</summary>
    public Result Verify(
        string accountsUserId, string? comment,
        IReadOnlyList<(string GlCode, string Description, decimal Debit, decimal Credit)> glLines)
    {
        if (Status != "Pending")
            return Result.Fail($"Only a Pending receipt can be verified (current status: {Status}).");
        if (string.Equals(accountsUserId, EnteredBy, StringComparison.OrdinalIgnoreCase))
            return Result.Fail("STRUCTURAL_VIOLATION: the Accounts verifier cannot be the CAD user who entered the receipt.");
        if (glLines.Count == 0)
            return Result.Fail("At least one GL line is required to verify a receipt.");

        var totalDebit = glLines.Sum(l => l.Debit);
        var totalCredit = glLines.Sum(l => l.Credit);
        if (Math.Round(totalDebit - totalCredit, 2) != 0)
            return Result.Fail($"GL is not balanced: debits {totalDebit:N2} != credits {totalCredit:N2}.");

        _glLines.Clear();
        foreach (var l in glLines)
            _glLines.Add(ReceiptGlLine.Create(Guid.NewGuid(), Id, l.GlCode, l.Description, l.Debit, l.Credit));

        VerifiedBy = accountsUserId;
        VerifiedAtUtc = DateTime.UtcNow;
        VerifyComment = comment;
        Status = "Verified";
        LastModifiedBy = accountsUserId;
        LastModifiedAtUtc = DateTime.UtcNow;
        return Result.Success();
    }
}
