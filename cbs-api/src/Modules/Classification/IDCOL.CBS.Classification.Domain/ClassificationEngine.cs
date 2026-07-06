namespace IDCOL.CBS.Classification.Domain;

public sealed record AccountClassificationResult(
    string Status,
    string ProvisionType,
    decimal ProvisionRatePercent,
    decimal ProvisionBase,
    decimal ProvisionRequired);

/// <summary>
/// The Bangladesh Bank DFIM Circular 04/2021 classification &amp; provisioning engine. Pure and
/// deterministic: given the config thresholds/rates and an account's finance type, tenor, period
/// of arrears and balances, it produces the classification status and the required provision. All
/// the regulatory numbers come from the passed-in config rows, never hardcoded here.
/// </summary>
public static class ClassificationEngine
{
    /// <summary>
    /// Objective (arrears-based) classification. A qualitative override may force a more severe
    /// status (documentation failure, fraud, collateral deterioration, etc.) but never a better one.
    /// </summary>
    public static string Classify(
        string financeType,
        int tenorMonths,
        decimal overdueMonths,
        IReadOnlyList<ClassificationThreshold> thresholds,
        string? qualitativeOverride = null)
    {
        var bucket = TenorBucket.For(financeType, tenorMonths);

        var relevant = thresholds
            .Where(t => t.FinanceType == financeType && t.TenorBucket == bucket)
            .OrderByDescending(t => t.MinOverdueMonths)
            .ToList();

        // Worst band whose lower bound the arrears reach; if none, the account is Standard.
        var objective = relevant.FirstOrDefault(t => t.Matches(overdueMonths))?.Status
            ?? ClassificationStatus.Standard;

        if (qualitativeOverride is null) return objective;

        var objectiveRank = ClassificationStatus.Severity.ToList().IndexOf(objective);
        var overrideRank = ClassificationStatus.Severity.ToList().IndexOf(qualitativeOverride);
        return overrideRank > objectiveRank ? qualitativeOverride : objective;
    }

    public static AccountClassificationResult ComputeProvision(
        string status,
        bool isCmsme,
        decimal outstanding,
        decimal interestSuspense,
        decimal eligibleCollateral,
        IReadOnlyList<ProvisioningRate> rates)
    {
        var rate = ResolveRate(status, isCmsme, rates);

        decimal provisionBase;
        if (ClassificationStatus.IsClassified(status))
        {
            // Specific provision base = higher of (outstanding - suspense - eligible collateral)
            // or 15% of outstanding (the regulatory floor).
            var net = outstanding - interestSuspense - eligibleCollateral;
            provisionBase = Math.Max(net, 0.15m * outstanding);
        }
        else if (status == ClassificationStatus.SpecialMention)
        {
            provisionBase = outstanding - interestSuspense; // general provision, net of suspense
        }
        else
        {
            provisionBase = outstanding; // Standard general provision on outstanding
        }

        provisionBase = Math.Max(0, provisionBase);
        var required = Math.Round(provisionBase * rate.RatePercent / 100m, 2);

        return new AccountClassificationResult(status, rate.ProvisionType, rate.RatePercent, provisionBase, required);
    }

    private static ProvisioningRate ResolveRate(string status, bool isCmsme, IReadOnlyList<ProvisioningRate> rates)
    {
        // Standard distinguishes CMSME (0.25%) from other (1%); the rest ignore the CMSME flag.
        if (status == ClassificationStatus.Standard)
        {
            return rates.FirstOrDefault(r => r.Status == status && r.IsCmsme == isCmsme)
                ?? rates.First(r => r.Status == status);
        }

        return rates.First(r => r.Status == status);
    }
}
