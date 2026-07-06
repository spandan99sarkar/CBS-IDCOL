namespace IDCOL.CBS.Classification.Domain;

/// <summary>
/// The DFIM Circular 04/2021 classification thresholds and provisioning rates as data. Used to
/// seed the config tables and by the engine tests. If Bangladesh Bank revises the matrix, these
/// rows change (or new effective-dated rows are added) - the engine code does not.
/// </summary>
public static class DfimSeed
{
    private const string Ref = "DFIM Circular 04/2021";
    private static readonly DateOnly Effective = new(2021, 9, 30);

    public static IReadOnlyList<ClassificationThreshold> Thresholds()
    {
        var list = new List<ClassificationThreshold>();

        void Band(string finance, string? bucket, string status, decimal min, decimal? max) =>
            list.Add(ClassificationThreshold.Create(Guid.NewGuid(), finance, bucket, status, min, max, Ref, Effective));

        // Short Term Finance (repayable within 12 months) - not tenor-bucketed.
        Band(FinanceType.ShortTerm, null, ClassificationStatus.SpecialMention, 2, 3);
        Band(FinanceType.ShortTerm, null, ClassificationStatus.SubStandard, 3, 6);
        Band(FinanceType.ShortTerm, null, ClassificationStatus.Doubtful, 6, 9);
        Band(FinanceType.ShortTerm, null, ClassificationStatus.BadLoss, 9, null);

        // Term & Lease Finance share the same thresholds, split at 5-year tenor.
        foreach (var finance in new[] { FinanceType.Term, FinanceType.Lease })
        {
            Band(finance, TenorBucket.UpToFiveYears, ClassificationStatus.SpecialMention, 3, 6);
            Band(finance, TenorBucket.UpToFiveYears, ClassificationStatus.SubStandard, 6, 12);
            Band(finance, TenorBucket.UpToFiveYears, ClassificationStatus.Doubtful, 12, 18);
            Band(finance, TenorBucket.UpToFiveYears, ClassificationStatus.BadLoss, 18, null);

            Band(finance, TenorBucket.OverFiveYears, ClassificationStatus.SpecialMention, 6, 12);
            Band(finance, TenorBucket.OverFiveYears, ClassificationStatus.SubStandard, 12, 18);
            Band(finance, TenorBucket.OverFiveYears, ClassificationStatus.Doubtful, 18, 24);
            Band(finance, TenorBucket.OverFiveYears, ClassificationStatus.BadLoss, 24, null);
        }

        // Housing Finance.
        Band(FinanceType.Housing, TenorBucket.UpToFiveYears, ClassificationStatus.SpecialMention, 9, 12);
        Band(FinanceType.Housing, TenorBucket.UpToFiveYears, ClassificationStatus.SubStandard, 12, 18);
        Band(FinanceType.Housing, TenorBucket.UpToFiveYears, ClassificationStatus.Doubtful, 18, 24);
        Band(FinanceType.Housing, TenorBucket.UpToFiveYears, ClassificationStatus.BadLoss, 24, null);

        Band(FinanceType.Housing, TenorBucket.OverFiveYears, ClassificationStatus.SpecialMention, 9, 18);
        Band(FinanceType.Housing, TenorBucket.OverFiveYears, ClassificationStatus.SubStandard, 18, 24);
        Band(FinanceType.Housing, TenorBucket.OverFiveYears, ClassificationStatus.Doubtful, 24, 36);
        Band(FinanceType.Housing, TenorBucket.OverFiveYears, ClassificationStatus.BadLoss, 36, null);

        return list;
    }

    public static IReadOnlyList<ProvisioningRate> Rates() => new List<ProvisioningRate>
    {
        ProvisioningRate.Create(Guid.NewGuid(), ClassificationStatus.Standard, isCmsme: true, "General", 0.25m, Ref, Effective),
        ProvisioningRate.Create(Guid.NewGuid(), ClassificationStatus.Standard, isCmsme: false, "General", 1m, Ref, Effective),
        ProvisioningRate.Create(Guid.NewGuid(), ClassificationStatus.SpecialMention, isCmsme: false, "General", 5m, Ref, Effective),
        ProvisioningRate.Create(Guid.NewGuid(), ClassificationStatus.SubStandard, isCmsme: false, "Specific", 20m, Ref, Effective),
        ProvisioningRate.Create(Guid.NewGuid(), ClassificationStatus.Doubtful, isCmsme: false, "Specific", 50m, Ref, Effective),
        ProvisioningRate.Create(Guid.NewGuid(), ClassificationStatus.BadLoss, isCmsme: false, "Specific", 100m, Ref, Effective),
    };
}
