namespace IDCOL.CBS.Security.Domain;

/// <summary>
/// Security and Covenant are one register split by this discriminator - the legacy "Loan Security
/// &amp; Covenant" module uses a single grid with a Category radio, so we model one aggregate.
/// </summary>
public static class SecurityCategory
{
    public const string Security = "Security";
    public const string Covenant = "Covenant";
}

/// <summary>The collateral / covenant instrument families IDCOL tracks (field "Instrument Type").</summary>
public static class InstrumentFamily
{
    public const string BankGuarantee = "BankGuarantee";
    public const string FDR = "FDR";
    public const string MTDR = "MTDR";
    public const string DSRA = "DSRA";
    public const string LandMortgage = "LandMortgage";
    public const string InsurancePolicy = "InsurancePolicy";
    public const string CreditRating = "CreditRating";
    public const string FinancialStatement = "FinancialStatement";
    public const string PDC = "PDC";
    public const string MonitoringFee = "MonitoringFee";

    public static readonly IReadOnlyList<string> All = new[]
    {
        BankGuarantee, FDR, MTDR, DSRA, LandMortgage, InsurancePolicy,
        CreditRating, FinancialStatement, PDC, MonitoringFee,
    };
}

/// <summary>Lifecycle status of an instrument (the grid "Status" column).</summary>
public static class InstrumentLifecycleState
{
    public const string Pending = "Pending";     // entered, awaiting verification
    public const string Live = "Live";           // active, lien confirmed
    public const string Expired = "Expired";
    public const string Renewed = "Renewed";
    public const string Encashed = "Encashed";
    public const string PartiallyEncashed = "PartiallyEncashed";
    public const string Returned = "Returned";   // released back to borrower
    public const string Revoked = "Revoked";     // lien revoked
    public const string Enforced = "Enforced";   // BG demand / enforcement invoked
}

/// <summary>
/// System-derived action for the dashboard, computed from days-to-expiry. This is the value the
/// legacy "Recommended Action" column shows and what drives reminder-letter generation.
/// </summary>
public static class RecommendedAction
{
    public const string NoActionRequired = "No action required";
    public const string FollowUpWithRM = "Follow up with RM";
    public const string SendRenewalInstruction = "Send renewal instruction";
    public const string SendReminderLetter = "Send reminder letter";
    public const string Expired = "Expired - escalate";
}

/// <summary>Covenant compliance state (colour-coded in the covenant/FS reports).</summary>
public static class ComplianceStatus
{
    public const string Complied = "Complied";
    public const string NotComplied = "Not Complied";
    public const string PendingReminder = "Pending Reminder";
    public const string NotApplicable = "N/A";
}
