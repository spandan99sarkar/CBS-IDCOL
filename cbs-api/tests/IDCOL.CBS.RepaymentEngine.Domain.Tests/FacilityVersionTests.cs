using System.Text.Json;
using IDCOL.CBS.RepaymentEngine.Domain;
using Xunit;

namespace IDCOL.CBS.RepaymentEngine.Domain.Tests;

public class FacilityVersionTests
{
    private static string SimpleParamsJson(double loanAmount, double rate, int installments, double disbDate, int stepMonths = 3)
    {
        var payDates = new List<double>();
        var cur = disbDate;
        for (var i = 0; i < installments; i++)
        {
            cur += stepMonths * 30; // simple fixed-step approximation, fine for a domain-layer test
            payDates.Add(cur);
        }

        var p = new ScheduleParameters
        {
            ProjectName = "Test Project",
            Currency = "BDT",
            LoanAmount = loanAmount,
            InterestRate = rate,
            DayCountBasis = 360,
            NumInstallments = installments,
            PrincipalType = "Level Principal",
            PaymentFrequency = 4,
            Disbursements = new List<Disbursement> { new() { DateSerial = disbDate, Amount = loanAmount } },
            RepaymentDates = payDates,
        };
        return JsonSerializer.Serialize(p);
    }

    [Fact]
    public void CreateOriginal_ProducesVersionZeroActive()
    {
        var facility = Facility.CreateOriginal(
            Guid.NewGuid(), Guid.NewGuid(), "IDCOL", "BDT", new DateOnly(2015, 1, 1),
            SimpleParamsJson(1_000_000, 0.09, 4, 42005), "cad1");

        Assert.Single(facility.Versions);
        var v0 = facility.CurrentVersion;
        Assert.Equal(0, v0.VersionSequence);
        Assert.Equal(FacilityVersionEventType.Original, v0.EventType);
        Assert.Equal(FacilityVersionStatus.Active, v0.Status);
    }

    [Fact]
    public void AddVersion_SupersedesPreviousAndBecomesCurrent()
    {
        var facility = Facility.CreateOriginal(
            Guid.NewGuid(), Guid.NewGuid(), "IDCOL", "BDT", new DateOnly(2015, 1, 1),
            SimpleParamsJson(1_000_000, 0.09, 4, 42005), "cad1");
        var original = facility.CurrentVersion;

        var result = facility.AddVersion(
            Guid.NewGuid(), FacilityVersionEventType.Reschedule, new DateOnly(2020, 1, 1), "1st Reschedule",
            "borrower_1st_reschedule.xls", rateBeforePercent: 9m, rateAfterPercent: 7.5m,
            tenorMonthsBefore: 96, tenorMonthsAfter: 120, capitalizedAmount: 50_000m, waivedAmount: 0m,
            overdueAmountRolledIn: 20_000m, regulatoryReference: null,
            parametersJson: SimpleParamsJson(800_000, 0.075, 6, 43831), createdBy: "cad1");

        Assert.True(result.IsSuccess);
        Assert.Equal(FacilityVersionStatus.Superseded, original.Status);
        Assert.Equal(2, facility.Versions.Count);
        Assert.Equal(1, facility.CurrentVersion.VersionSequence);
        Assert.Equal(FacilityVersionEventType.Reschedule, facility.CurrentVersion.EventType);
        Assert.Equal(50_000m, facility.CurrentVersion.CapitalizedAmount);
        Assert.Equal(20_000m, facility.CurrentVersion.OverdueAmountRolledIn);
    }

    [Fact]
    public void AddVersion_WithEarlierEffectiveDate_Fails()
    {
        var facility = Facility.CreateOriginal(
            Guid.NewGuid(), Guid.NewGuid(), "IDCOL", "BDT", new DateOnly(2020, 1, 1),
            SimpleParamsJson(1_000_000, 0.09, 4, 43831), "cad1");

        var result = facility.AddVersion(
            Guid.NewGuid(), FacilityVersionEventType.Restructure, new DateOnly(2019, 1, 1), "Bad date",
            null, null, null, null, null, 0, 0, 0, null,
            SimpleParamsJson(800_000, 0.075, 6, 43466), "cad1");

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void ComputeSchedule_ReturnsRowsMatchingNumInstallments()
    {
        var facility = Facility.CreateOriginal(
            Guid.NewGuid(), Guid.NewGuid(), "IDCOL", "BDT", new DateOnly(2015, 1, 1),
            SimpleParamsJson(1_000_000, 0.09, 4, 42005), "cad1");

        var rows = facility.CurrentVersion.ComputeSchedule();

        Assert.Equal(4, rows.Count);
        Assert.Equal(1_000_000, rows[0].OpeningBal);
    }

    [Fact]
    public void ApplyInstallmentOverride_ChangesOnlyTargetRow_OthersKeepNaturalValues()
    {
        var facility = Facility.CreateOriginal(
            Guid.NewGuid(), Guid.NewGuid(), "IDCOL", "BDT", new DateOnly(2015, 1, 1),
            SimpleParamsJson(1_000_000, 0.09, 4, 42005), "cad1");
        var version = facility.CurrentVersion;
        var naturalRows = version.ComputeSchedule();

        version.ApplyInstallmentOverride(1, interestOverride: 12_345m, null, null, "cad1");
        var overriddenRows = version.ComputeSchedule();

        Assert.Equal(12_345, overriddenRows[1].CashInterest);
        // Row 0, 2, 3 must be unaffected by overriding row 1 - this is the exact bug class
        // (silent collapse to 0) the backfill logic in ApplyInstallmentOverride guards against.
        Assert.Equal(naturalRows[0].CashInterest, overriddenRows[0].CashInterest, precision: 6);
        Assert.Equal(naturalRows[2].CashInterest, overriddenRows[2].CashInterest, precision: 6);
        Assert.Equal(naturalRows[3].CashInterest, overriddenRows[3].CashInterest, precision: 6);
    }

    [Fact]
    public void ApplyInstallmentOverride_OutOfRangeIndex_Throws()
    {
        var facility = Facility.CreateOriginal(
            Guid.NewGuid(), Guid.NewGuid(), "IDCOL", "BDT", new DateOnly(2015, 1, 1),
            SimpleParamsJson(1_000_000, 0.09, 4, 42005), "cad1");

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            facility.CurrentVersion.ApplyInstallmentOverride(10, 100m, null, null, "cad1"));
    }
}
