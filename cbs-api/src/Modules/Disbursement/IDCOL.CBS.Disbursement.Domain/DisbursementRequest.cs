using IDCOL.CBS.SharedKernel.Common;
using IDCOL.CBS.SharedKernel.Domain;

namespace IDCOL.CBS.Disbursement.Domain;

/// <summary>
/// A tranche disbursement moving through IDCOL's 3-stage maker-checker workflow:
/// Suggested (BU initiates) -> Proposed (CAD reviews/justifies) -> Processed (Accounts posts to GL).
///
/// The structural control the Bangladesh Bank NBFI guideline requires ("a user cannot hold dual
/// roles") is enforced here in the domain, not just the UI: each stage records who performed it,
/// and a stage transition is rejected if the acting user already performed an earlier stage of the
/// same request. Role gating (BU vs CAD vs Accounts) is enforced at the application layer.
/// </summary>
public class DisbursementRequest : AggregateRoot<Guid>, IAuditable
{
    public string ReferenceNo { get; private set; } = default!;
    public int DisbursementNo { get; private set; } = 1;

    public Guid SanctionId { get; private set; }
    public string SanctionRef { get; private set; } = default!;
    public string CustomerNo { get; private set; } = default!;
    public string ProjectName { get; private set; } = default!;
    public string LoanCurrency { get; private set; } = "BDT";

    public string Status { get; private set; } = "Suggested"; // Suggested | Proposed | Processed

    // Stage 1 - BU initiate
    public decimal SuggestedLoanAmount { get; private set; }
    public decimal SuggestedGrantAmount { get; private set; }
    public string? BuRemarks { get; private set; }
    public string InitiatedBy { get; private set; } = default!;
    public DateTime InitiatedAtUtc { get; private set; }

    // Stage 2 - CAD review/justify
    public decimal? JustifiedLoanAmount { get; private set; }
    public decimal? JustifiedGrantAmount { get; private set; }
    public string? CadRemarks { get; private set; }
    public string? ProposedBy { get; private set; }
    public DateTime? ProposedAtUtc { get; private set; }

    // Stage 3 - Accounts post
    public string? DisbursementMode { get; private set; } // Cheque | EFT | RTGS | PayOrder | SWIFT
    public DateOnly? ValueDate { get; private set; }
    public string? PostedBy { get; private set; }
    public DateTime? PostedAtUtc { get; private set; }

    private readonly List<DisbursementGlLine> _glLines = new();
    public IReadOnlyCollection<DisbursementGlLine> GlLines => _glLines.AsReadOnly();

    public string CreatedBy { get; private set; } = default!;
    public DateTime CreatedAtUtc { get; private set; }
    public string? LastModifiedBy { get; private set; }
    public DateTime? LastModifiedAtUtc { get; private set; }

    private DisbursementRequest()
    {
    }

    /// <summary>Stage 1: BU initiates a disbursement against a signed sanction.</summary>
    public static DisbursementRequest Initiate(
        Guid id, string referenceNo, int disbursementNo, Guid sanctionId, string sanctionRef,
        string customerNo, string projectName, string loanCurrency, decimal suggestedLoanAmount,
        decimal suggestedGrantAmount, string? buRemarks, string initiatingUserId)
    {
        if (string.IsNullOrWhiteSpace(referenceNo)) throw new ArgumentException("Reference no is required.", nameof(referenceNo));
        if (suggestedLoanAmount < 0 || suggestedGrantAmount < 0) throw new ArgumentException("Amounts cannot be negative.");
        if (suggestedLoanAmount == 0 && suggestedGrantAmount == 0) throw new ArgumentException("At least one of loan/grant amount must be positive.");

        return new DisbursementRequest
        {
            Id = id,
            ReferenceNo = referenceNo,
            DisbursementNo = disbursementNo,
            SanctionId = sanctionId,
            SanctionRef = sanctionRef,
            CustomerNo = customerNo,
            ProjectName = projectName,
            LoanCurrency = loanCurrency,
            Status = "Suggested",
            SuggestedLoanAmount = suggestedLoanAmount,
            SuggestedGrantAmount = suggestedGrantAmount,
            BuRemarks = buRemarks,
            InitiatedBy = initiatingUserId,
            InitiatedAtUtc = DateTime.UtcNow,
            CreatedBy = initiatingUserId,
            CreatedAtUtc = DateTime.UtcNow,
        };
    }

    /// <summary>Stage 2: CAD reviews and (optionally) adjusts the amounts, moving to Proposed.</summary>
    public Result Propose(string cadUserId, decimal justifiedLoanAmount, decimal justifiedGrantAmount, string? cadRemarks)
    {
        if (Status != "Suggested")
            return Result.Fail($"Only a Suggested disbursement can be reviewed by CAD (current status: {Status}).");
        if (string.Equals(cadUserId, InitiatedBy, StringComparison.OrdinalIgnoreCase))
            return Result.Fail("STRUCTURAL_VIOLATION: the CAD reviewer cannot be the BU initiator of the same disbursement.");
        if (justifiedLoanAmount < 0 || justifiedGrantAmount < 0)
            return Result.Fail("Justified amounts cannot be negative.");

        JustifiedLoanAmount = justifiedLoanAmount;
        JustifiedGrantAmount = justifiedGrantAmount;
        CadRemarks = cadRemarks;
        ProposedBy = cadUserId;
        ProposedAtUtc = DateTime.UtcNow;
        Status = "Proposed";
        Touch(cadUserId);
        return Result.Success();
    }

    /// <summary>Stage 3: Accounts posts the disbursement to the GL, moving to Processed.</summary>
    public Result Post(
        string accountsUserId, string disbursementMode, DateOnly valueDate,
        IReadOnlyList<(string GlCode, string Description, decimal Debit, decimal Credit)> glLines)
    {
        if (Status != "Proposed")
            return Result.Fail($"Only a Proposed disbursement can be posted by Accounts (current status: {Status}).");
        if (string.Equals(accountsUserId, InitiatedBy, StringComparison.OrdinalIgnoreCase))
            return Result.Fail("STRUCTURAL_VIOLATION: the Accounts poster cannot be the BU initiator of the same disbursement.");
        if (string.Equals(accountsUserId, ProposedBy, StringComparison.OrdinalIgnoreCase))
            return Result.Fail("STRUCTURAL_VIOLATION: the Accounts poster cannot be the CAD reviewer of the same disbursement.");
        if (glLines.Count == 0)
            return Result.Fail("At least one GL line is required to post a disbursement.");

        var totalDebit = glLines.Sum(l => l.Debit);
        var totalCredit = glLines.Sum(l => l.Credit);
        if (Math.Round(totalDebit - totalCredit, 2) != 0)
            return Result.Fail($"GL is not balanced: debits {totalDebit:N2} != credits {totalCredit:N2}.");

        _glLines.Clear();
        foreach (var l in glLines)
            _glLines.Add(DisbursementGlLine.Create(Guid.NewGuid(), Id, l.GlCode, l.Description, l.Debit, l.Credit));

        DisbursementMode = disbursementMode;
        ValueDate = valueDate;
        PostedBy = accountsUserId;
        PostedAtUtc = DateTime.UtcNow;
        Status = "Processed";
        Touch(accountsUserId);
        return Result.Success();
    }

    /// <summary>The amount that will actually disburse = CAD's justified figure once reviewed, else BU's suggestion.</summary>
    public decimal EffectiveLoanAmount => JustifiedLoanAmount ?? SuggestedLoanAmount;
    public decimal EffectiveGrantAmount => JustifiedGrantAmount ?? SuggestedGrantAmount;

    private void Touch(string modifiedBy)
    {
        LastModifiedBy = modifiedBy;
        LastModifiedAtUtc = DateTime.UtcNow;
    }
}
