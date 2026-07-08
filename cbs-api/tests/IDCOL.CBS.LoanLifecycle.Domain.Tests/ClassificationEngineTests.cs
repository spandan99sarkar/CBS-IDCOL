using IDCOL.CBS.Classification.Domain;
using Xunit;

namespace IDCOL.CBS.LoanLifecycle.Domain.Tests;

public class ClassificationEngineTests
{
    private static readonly IReadOnlyList<ClassificationThreshold> Thresholds = DfimSeed.Thresholds();
    private static readonly IReadOnlyList<ProvisioningRate> Rates = DfimSeed.Rates();

    // DFIM 04/2021 Term Finance, >5yr tenor: SMA >=6, SS >=12, DF >=18, BL >=24 months overdue.
    [Theory]
    [InlineData(0, ClassificationStatus.Standard)]
    [InlineData(5, ClassificationStatus.Standard)]
    [InlineData(6, ClassificationStatus.SpecialMention)]
    [InlineData(11.9, ClassificationStatus.SpecialMention)]
    [InlineData(12, ClassificationStatus.SubStandard)]
    [InlineData(17, ClassificationStatus.SubStandard)]
    [InlineData(18, ClassificationStatus.Doubtful)]
    [InlineData(23, ClassificationStatus.Doubtful)]
    [InlineData(24, ClassificationStatus.BadLoss)]
    [InlineData(40, ClassificationStatus.BadLoss)]
    public void Classify_TermFinanceOverFiveYears_MatchesDfimBands(decimal overdueMonths, string expected)
    {
        var status = ClassificationEngine.Classify(FinanceType.Term, tenorMonths: 96, overdueMonths, Thresholds);
        Assert.Equal(expected, status);
    }

    // Term Finance, <=5yr tenor: SMA >=3, SS >=6, DF >=12, BL >=18.
    [Theory]
    [InlineData(2, ClassificationStatus.Standard)]
    [InlineData(3, ClassificationStatus.SpecialMention)]
    [InlineData(6, ClassificationStatus.SubStandard)]
    [InlineData(12, ClassificationStatus.Doubtful)]
    [InlineData(18, ClassificationStatus.BadLoss)]
    public void Classify_TermFinanceUpToFiveYears_MatchesDfimBands(decimal overdueMonths, string expected)
    {
        var status = ClassificationEngine.Classify(FinanceType.Term, tenorMonths: 48, overdueMonths, Thresholds);
        Assert.Equal(expected, status);
    }

    // Short Term Finance: SMA >=2, SS >=3, DF >=6, BL >=9.
    [Theory]
    [InlineData(1, ClassificationStatus.Standard)]
    [InlineData(2, ClassificationStatus.SpecialMention)]
    [InlineData(3, ClassificationStatus.SubStandard)]
    [InlineData(6, ClassificationStatus.Doubtful)]
    [InlineData(9, ClassificationStatus.BadLoss)]
    public void Classify_ShortTermFinance_MatchesDfimBands(decimal overdueMonths, string expected)
    {
        var status = ClassificationEngine.Classify(FinanceType.ShortTerm, tenorMonths: 9, overdueMonths, Thresholds);
        Assert.Equal(expected, status);
    }

    [Fact]
    public void Classify_QualitativeOverride_TakesMoreSevereStatus()
    {
        // Objective would be Standard (0 overdue), but a qualitative Sub-Standard override wins.
        var status = ClassificationEngine.Classify(
            FinanceType.Term, 96, overdueMonths: 0, Thresholds, qualitativeOverride: ClassificationStatus.SubStandard);
        Assert.Equal(ClassificationStatus.SubStandard, status);
    }

    [Fact]
    public void Classify_QualitativeOverride_NeverImprovesStatus()
    {
        // Objective Bad/Loss cannot be softened to Standard by an override.
        var status = ClassificationEngine.Classify(
            FinanceType.Term, 96, overdueMonths: 30, Thresholds, qualitativeOverride: ClassificationStatus.Standard);
        Assert.Equal(ClassificationStatus.BadLoss, status);
    }

    [Fact]
    public void Provision_Standard_IsOnePercentOfOutstanding()
    {
        var r = ClassificationEngine.ComputeProvision(
            ClassificationStatus.Standard, isCmsme: false, outstanding: 1_000_000m,
            interestSuspense: 0, eligibleCollateral: 0, Rates);

        Assert.Equal("General", r.ProvisionType);
        Assert.Equal(1m, r.ProvisionRatePercent);
        Assert.Equal(10_000m, r.ProvisionRequired);
    }

    [Fact]
    public void Provision_StandardCmsme_IsQuarterPercent()
    {
        var r = ClassificationEngine.ComputeProvision(
            ClassificationStatus.Standard, isCmsme: true, outstanding: 1_000_000m, 0, 0, Rates);

        Assert.Equal(0.25m, r.ProvisionRatePercent);
        Assert.Equal(2_500m, r.ProvisionRequired);
    }

    [Fact]
    public void Provision_BadLoss_Uses15PercentFloorWhenCollateralExceedsNet()
    {
        // Outstanding 1,000,000 fully collateralised: net = 0, but the base floors at 15% = 150,000,
        // so Bad/Loss (100%) requires 150,000.
        var r = ClassificationEngine.ComputeProvision(
            ClassificationStatus.BadLoss, isCmsme: false, outstanding: 1_000_000m,
            interestSuspense: 0, eligibleCollateral: 1_000_000m, Rates);

        Assert.Equal("Specific", r.ProvisionType);
        Assert.Equal(100m, r.ProvisionRatePercent);
        Assert.Equal(150_000m, r.ProvisionBase);
        Assert.Equal(150_000m, r.ProvisionRequired);
    }

    [Fact]
    public void Provision_Doubtful_NetOfSuspenseAndCollateral()
    {
        // Base = max(1,000,000 - 100,000 suspense - 300,000 collateral, 150,000) = 600,000; DF 50% = 300,000.
        var r = ClassificationEngine.ComputeProvision(
            ClassificationStatus.Doubtful, isCmsme: false, outstanding: 1_000_000m,
            interestSuspense: 100_000m, eligibleCollateral: 300_000m, Rates);

        Assert.Equal(600_000m, r.ProvisionBase);
        Assert.Equal(300_000m, r.ProvisionRequired);
    }
}
