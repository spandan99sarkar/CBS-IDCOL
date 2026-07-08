namespace IDCOL.CBS.Classification.Domain;

/// <summary>The five Bangladesh Bank loan-classification statuses, worst-last.</summary>
public static class ClassificationStatus
{
    public const string Standard = "Standard";
    public const string SpecialMention = "SMA"; // Special Mention Account
    public const string SubStandard = "Sub-Standard";
    public const string Doubtful = "Doubtful";
    public const string BadLoss = "Bad/Loss";

    /// <summary>Ordered best-to-worst, used for qualitative-downgrade comparisons.</summary>
    public static readonly IReadOnlyList<string> Severity = new[]
    {
        Standard, SpecialMention, SubStandard, Doubtful, BadLoss,
    };

    public static bool IsClassified(string status) =>
        status is SubStandard or Doubtful or BadLoss;
}

/// <summary>DFIM Circular 04/2021 finance-type buckets (each has its own overdue thresholds).</summary>
public static class FinanceType
{
    public const string ShortTerm = "ShortTerm"; // repayable within 12 months
    public const string Term = "Term";
    public const string Lease = "Lease";
    public const string Housing = "Housing";
}

public static class TenorBucket
{
    public const string UpToFiveYears = "1-5YR";
    public const string OverFiveYears = ">5YR";

    /// <summary>Short-term finance is not tenor-bucketed; term/lease/housing split at 5 years.</summary>
    public static string? For(string financeType, int tenorMonths) =>
        financeType == FinanceType.ShortTerm ? null : (tenorMonths <= 60 ? UpToFiveYears : OverFiveYears);
}
