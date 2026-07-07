using IDCOL.CBS.SharedKernel.Common;
using IDCOL.CBS.SharedKernel.Domain;

namespace IDCOL.CBS.Security.Domain;

/// <summary>
/// One security-collateral or covenant instrument attached to a loan - a bank guarantee, lien-marked
/// FDR/MTDR, DSRA, land mortgage, insurance policy, credit-rating obligation, financial-statement
/// obligation, PDC, or monitoring fee. Faithful to IDCOL's legacy "Loan Security &amp; Covenant"
/// register: a single table discriminated by <see cref="SecurityCategory"/>, with an expiry-driven
/// "recommended action" engine that drives the dashboard highlighting and reminder letters.
///
/// Valuation haircuts ("eligible as security % as per BB", "IDCOL portion %") are kept as stored
/// input percentages with the resulting amounts computed on read, never persisted.
/// </summary>
public class SecurityCovenantInstrument : AggregateRoot<Guid>, IAuditable
{
    public string Category { get; private set; } = SecurityCategory.Security;
    public string InstrumentFamily { get; private set; } = default!;
    public string? LoanType { get; private set; }

    // Loan linkage (denormalised for the register grid).
    public Guid SanctionId { get; private set; }
    public string ClientName { get; private set; } = default!;   // borrower / customer no
    public string ProjectName { get; private set; } = default!;
    public string? StatementName { get; private set; }

    public string? InstrumentNumber { get; private set; }        // BG no / FDR no / policy no / cheque no
    public string? IssuingBank { get; private set; }
    public string? IssuingBranch { get; private set; }
    public string Currency { get; private set; } = "BDT";

    public decimal LeafValueOrInitialAmount { get; private set; }
    public decimal CurrentBalance { get; private set; }          // FDR/MTDR balance, refreshed via UpdateAmountAndMaturity

    public DateOnly? IssueDate { get; private set; }             // or lien-acknowledgement date
    public DateOnly? ExpiryDate { get; private set; }            // or maturity date

    public string VerificationStatus { get; private set; } = "Pending"; // Verified | Pending
    public bool AutoRenewal { get; private set; }
    public string LifecycleState { get; private set; } = InstrumentLifecycleState.Pending;
    public string? ActionTaken { get; private set; }
    public string? Remarks { get; private set; }

    // Valuation / eligibility (mainly for Category = Covenant land/security).
    public decimal? MarketValue { get; private set; }
    public decimal? ForcedSaleValue { get; private set; }        // or sum insured
    public decimal IdcolPortionPercent { get; private set; }
    public decimal EligibleSecurityPercent { get; private set; } // as per Bangladesh Bank
    public string? Provider { get; private set; }                // valuator / CR agency / insurer
    public string? Rating { get; private set; }
    public string? Location { get; private set; }

    // Covenant compliance (for Category = Covenant).
    public string? CovenantType { get; private set; }
    public string? ComplianceStatus { get; private set; }
    public DateOnly? NextDueDate { get; private set; }

    public string CreatedBy { get; private set; } = default!;
    public DateTime CreatedAtUtc { get; private set; }
    public string? LastModifiedBy { get; private set; }
    public DateTime? LastModifiedAtUtc { get; private set; }

    private SecurityCovenantInstrument()
    {
    }

    public static SecurityCovenantInstrument Create(
        Guid id, string category, string instrumentFamily, string? loanType, Guid sanctionId,
        string clientName, string projectName, string? statementName, string? instrumentNumber,
        string? issuingBank, string? issuingBranch, string currency, decimal leafValueOrInitialAmount,
        decimal currentBalance, DateOnly? issueDate, DateOnly? expiryDate, string verificationStatus,
        bool autoRenewal, string lifecycleState, decimal? marketValue, decimal? forcedSaleValue,
        decimal idcolPortionPercent, decimal eligibleSecurityPercent, string? provider, string? rating,
        string? location, string? covenantType, string? complianceStatus, DateOnly? nextDueDate,
        string? remarks, string createdBy)
    {
        if (string.IsNullOrWhiteSpace(instrumentFamily))
            throw new ArgumentException("Instrument family is required.", nameof(instrumentFamily));

        return new SecurityCovenantInstrument
        {
            Id = id,
            Category = category,
            InstrumentFamily = instrumentFamily,
            LoanType = loanType,
            SanctionId = sanctionId,
            ClientName = clientName,
            ProjectName = projectName,
            StatementName = statementName,
            InstrumentNumber = instrumentNumber,
            IssuingBank = issuingBank,
            IssuingBranch = issuingBranch,
            Currency = currency,
            LeafValueOrInitialAmount = leafValueOrInitialAmount,
            CurrentBalance = currentBalance,
            IssueDate = issueDate,
            ExpiryDate = expiryDate,
            VerificationStatus = verificationStatus,
            AutoRenewal = autoRenewal,
            LifecycleState = lifecycleState,
            MarketValue = marketValue,
            ForcedSaleValue = forcedSaleValue,
            IdcolPortionPercent = idcolPortionPercent,
            EligibleSecurityPercent = eligibleSecurityPercent,
            Provider = provider,
            Rating = rating,
            Location = location,
            CovenantType = covenantType,
            ComplianceStatus = complianceStatus,
            NextDueDate = nextDueDate,
            Remarks = remarks,
            CreatedBy = createdBy,
            CreatedAtUtc = DateTime.UtcNow,
        };
    }

    /// <summary>Days until expiry as of a date (negative once expired). Null if the instrument has no expiry.</summary>
    public int? DaysLeft(DateOnly asOf) =>
        ExpiryDate.HasValue ? ExpiryDate.Value.DayNumber - asOf.DayNumber : null;

    /// <summary>
    /// The expiry-driven recommended action. Auto-renewing instruments never need a renewal
    /// instruction; anything already lapsed escalates; otherwise the reminder ladder kicks in as
    /// the expiry approaches.
    /// </summary>
    public string ComputeRecommendedAction(DateOnly asOf)
    {
        if (LifecycleState is InstrumentLifecycleState.Returned or InstrumentLifecycleState.Revoked
            or InstrumentLifecycleState.Encashed)
            return RecommendedAction.NoActionRequired;

        var days = DaysLeft(asOf);
        if (days is null) return RecommendedAction.NoActionRequired;
        if (days < 0) return RecommendedAction.Expired;
        if (days <= 15) return RecommendedAction.SendReminderLetter;
        if (days <= 45) return AutoRenewal ? RecommendedAction.FollowUpWithRM : RecommendedAction.SendRenewalInstruction;
        if (days <= 90) return RecommendedAction.FollowUpWithRM;
        return RecommendedAction.NoActionRequired;
    }

    /// <summary>IDCOL's share of the forced-sale value.</summary>
    public decimal ForcedSaleValueIdcolPortion =>
        Math.Round((ForcedSaleValue ?? 0m) * IdcolPortionPercent / 100m, 2);

    /// <summary>Amount admissible as security after the Bangladesh Bank eligibility haircut.</summary>
    public decimal EligibleAmount =>
        Math.Round(ForcedSaleValueIdcolPortion * EligibleSecurityPercent / 100m, 2);

    /// <summary>The lightweight "Update Amount and Maturity Date" action (FDR/MTDR balance refresh).</summary>
    public void UpdateAmountAndMaturity(decimal currentBalance, DateOnly? expiryDate, string modifiedBy)
    {
        CurrentBalance = currentBalance;
        if (expiryDate.HasValue) ExpiryDate = expiryDate;
        Touch(modifiedBy);
    }

    /// <summary>Records an operational outcome and moves the instrument to a new lifecycle state.</summary>
    public void RecordAction(string actionTaken, string newState, string? remarks, string modifiedBy)
    {
        ActionTaken = actionTaken;
        LifecycleState = newState;
        if (!string.IsNullOrWhiteSpace(remarks)) Remarks = remarks;
        Touch(modifiedBy);
    }

    private void Touch(string modifiedBy)
    {
        LastModifiedBy = modifiedBy;
        LastModifiedAtUtc = DateTime.UtcNow;
    }
}
