using IDCOL.CBS.RepaymentEngine.Domain;
using Xunit;

namespace IDCOL.CBS.RepaymentEngine.Domain.Tests;

public class InterestAccrualCalculatorTests
{
    [Fact]
    public void Accrue_SimpleQuarter_MatchesDayCountFormula()
    {
        // 100,000 at 6% over 90 days on a 360-day basis = 100000 * 0.06 * 90/360 = 1,500.
        var interest = InterestAccrualCalculator.Accrue(
            startDate: 1000, endDate: 1090, startBalance: 100_000,
            disbursements: new List<Disbursement>(), baseRate: 0.06, periodRate: 0.06,
            rateEvents: new List<RateChangeEvent>(), basis: 360);

        Assert.Equal(1500, interest, 6);
    }

    [Fact]
    public void Accrue_MidPeriodDisbursement_RaisesBalanceFromValueDate()
    {
        // 100k for the first 45 days, then +50k for the last 45 days, at 6%/360.
        // = 100000*0.06*45/360 + 150000*0.06*45/360 = 750 + 1125 = 1875.
        var interest = InterestAccrualCalculator.Accrue(
            startDate: 1000, endDate: 1090, startBalance: 100_000,
            disbursements: new List<Disbursement> { new() { DateSerial = 1045, Amount = 50_000 } },
            baseRate: 0.06, periodRate: 0.06, rateEvents: new List<RateChangeEvent>(), basis: 360);

        Assert.Equal(1875, interest, 6);
    }

    [Fact]
    public void Accrue_MidPeriodRateChange_AppliesNewRateProspectively()
    {
        // 100k at 6% for 45 days then 8% for 45 days = 750 + 100000*0.08*45/360 (1000) = 1750.
        var interest = InterestAccrualCalculator.Accrue(
            startDate: 1000, endDate: 1090, startBalance: 100_000,
            disbursements: new List<Disbursement>(), baseRate: 0.06, periodRate: 0.06,
            rateEvents: new List<RateChangeEvent> { new() { DateSerial = 1045, Rate = 0.08 } }, basis: 360);

        Assert.Equal(1750, interest, 6);
    }

    [Fact]
    public void GetRateAtDate_ReturnsMostRecentEffectiveRate()
    {
        var events = new List<RateChangeEvent>
        {
            new() { DateSerial = 1000, Rate = 0.09 },
            new() { DateSerial = 2000, Rate = 0.11 },
        };

        Assert.Equal(0.06, InterestAccrualCalculator.GetRateAtDate(0.06, events, 500));
        Assert.Equal(0.09, InterestAccrualCalculator.GetRateAtDate(0.06, events, 1500));
        Assert.Equal(0.11, InterestAccrualCalculator.GetRateAtDate(0.06, events, 2500));
    }
}
