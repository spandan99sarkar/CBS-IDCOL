using System.Text;

namespace IDCOL.CBS.Security.Domain;

public sealed record LetterTemplateInfo(string Family, string LetterType, string Purpose);

/// <summary>
/// The catalogue of ~30 templated letters IDCOL's Credit Administration generates against a
/// security/covenant instrument, and a merge renderer. Letter types double as lifecycle triggers
/// (generating "Revoke Lien" sets the instrument Revoked; "Encashment" sets it Encashed, etc.).
/// Reference numbers follow IDCOL's convention IDCOL/CAD/&lt;Unit&gt;/&lt;Borrower&gt;/&lt;Type&gt;/&lt;Year&gt;/&lt;Month&gt;/&lt;Seq&gt;.
/// </summary>
public static class SecurityLetterCatalog
{
    public static readonly IReadOnlyList<LetterTemplateInfo> Templates = new List<LetterTemplateInfo>
    {
        new(InstrumentFamily.BankGuarantee, "Confirmation", "Verify a bank guarantee with the issuing bank"),
        new(InstrumentFamily.BankGuarantee, "Renewal", "Request renewal of an expiring BG"),
        new(InstrumentFamily.BankGuarantee, "Renewal - Reduced Amount", "Renew a BG at a reduced amount"),
        new(InstrumentFamily.BankGuarantee, "Demand Notice", "Invoke/enforce the full BG amount"),
        new(InstrumentFamily.BankGuarantee, "Demand Notice - Partial", "Invoke part of the BG amount"),
        new(InstrumentFamily.BankGuarantee, "Return", "Return a BG no longer required"),
        new(InstrumentFamily.BankGuarantee, "Revocation Demand Notice", "Revoke a previously served demand notice"),
        new(InstrumentFamily.FDR, "Verification Request", "Verify an FDR with the bank"),
        new(InstrumentFamily.FDR, "Lien Creation", "Mark a lien on an FDR account"),
        new(InstrumentFamily.FDR, "Lien Reminder", "Remind the bank to acknowledge the FDR lien"),
        new(InstrumentFamily.FDR, "Encashment", "Encash a lien-marked FDR"),
        new(InstrumentFamily.FDR, "Partial Encashment", "Partially encash an FDR"),
        new(InstrumentFamily.FDR, "Revoke Lien", "Release the lien on an FDR"),
        new(InstrumentFamily.MTDR, "Verification Request", "Verify an MTDR with the bank"),
        new(InstrumentFamily.MTDR, "Lien Creation", "Mark a lien on an MTDR"),
        new(InstrumentFamily.MTDR, "Encashment", "Encash a lien-marked MTDR"),
        new(InstrumentFamily.MTDR, "Revoke Lien", "Release the lien on an MTDR"),
        new(InstrumentFamily.DSRA, "Reminder", "Remind the borrower to maintain the DSRA"),
        new(InstrumentFamily.DSRA, "Withdrawal of Fund", "Authorise a DSRA withdrawal"),
        new(InstrumentFamily.DSRA, "Revoke Lien", "Release the lien on the DSRA"),
        new(InstrumentFamily.LandMortgage, "1st Submission of Valuation Report", "Request the land valuation report"),
        new(InstrumentFamily.LandMortgage, "Reminder - Valuation Report", "Reminder for the land valuation report"),
        new(InstrumentFamily.InsurancePolicy, "1st Submission of Insurance Policy", "Request a valid insurance policy"),
        new(InstrumentFamily.InsurancePolicy, "Reminder - Insurance Policy", "Reminder for the insurance policy"),
        new(InstrumentFamily.CreditRating, "1st Submission of Credit Rating", "Request the latest credit-rating report"),
        new(InstrumentFamily.CreditRating, "Reminder - Credit Rating", "Reminder for the credit-rating report"),
        new(InstrumentFamily.FinancialStatement, "1st Submission of Audited FS", "Request audited financial statements"),
        new(InstrumentFamily.FinancialStatement, "Reminder - Audited FS", "Reminder for the audited financial statements"),
        new(InstrumentFamily.PDC, "Verification Request", "Verify signatures on post-dated cheques"),
        new(InstrumentFamily.MonitoringFee, "Payment of Annual Monitoring Fee", "Invoice the annual monitoring fee (incl. VAT)"),
    };

    public static IEnumerable<LetterTemplateInfo> ForFamily(string family) =>
        Templates.Where(t => t.Family == family);

    /// <summary>Renders a merged letter body for an instrument and letter type.</summary>
    public static string Render(SecurityCovenantInstrument i, string letterType, string refNo, DateOnly letterDate)
    {
        var amount = i.CurrentBalance > 0 ? i.CurrentBalance : i.LeafValueOrInitialAmount;
        var sb = new StringBuilder();
        sb.AppendLine($"Ref: {refNo}");
        sb.AppendLine($"Date: {letterDate:dd MMMM yyyy}");
        sb.AppendLine();
        sb.AppendLine("The Manager");
        sb.AppendLine(i.IssuingBank ?? "[Issuing Bank]");
        sb.AppendLine(i.IssuingBranch ?? "[Branch]");
        sb.AppendLine();
        sb.AppendLine($"Subject: {letterType} - {Describe(i.InstrumentFamily)} of {i.ClientName}");
        sb.AppendLine();
        sb.AppendLine("Dear Sir,");
        sb.AppendLine();
        sb.AppendLine(
            $"With reference to the {Describe(i.InstrumentFamily)} No. {i.InstrumentNumber ?? "[No.]"} " +
            $"for {i.Currency} {amount:N2} issued in favour of Infrastructure Development Company Limited (IDCOL) " +
            $"on account of {i.ClientName} ({i.ProjectName}), we hereby request you to proceed with the " +
            $"'{letterType}' as per the terms of the Financing Agreement.");
        sb.AppendLine();
        if (i.ExpiryDate.HasValue)
            sb.AppendLine($"The instrument's current expiry/maturity date is {i.ExpiryDate:dd MMMM yyyy}.");
        sb.AppendLine();
        sb.AppendLine("Thank you.");
        sb.AppendLine();
        sb.AppendLine("Yours faithfully,");
        sb.AppendLine("Assistant Vice President, Credit Administration      Unit Head, Credit Administration");
        sb.AppendLine("Infrastructure Development Company Limited (IDCOL)");
        return sb.ToString();
    }

    private static string Describe(string family) => family switch
    {
        InstrumentFamily.BankGuarantee => "Bank Guarantee",
        InstrumentFamily.FDR => "Fixed Deposit Receipt (FDR)",
        InstrumentFamily.MTDR => "Mudarabah Term Deposit Receipt (MTDR)",
        InstrumentFamily.DSRA => "Debt Service Reserve Account (DSRA)",
        InstrumentFamily.LandMortgage => "Land Mortgage",
        InstrumentFamily.InsurancePolicy => "Insurance Policy",
        InstrumentFamily.CreditRating => "Credit Rating Report",
        InstrumentFamily.FinancialStatement => "Audited Financial Statements",
        InstrumentFamily.PDC => "Post-Dated Cheque(s)",
        InstrumentFamily.MonitoringFee => "Monitoring Fee",
        _ => family,
    };
}
