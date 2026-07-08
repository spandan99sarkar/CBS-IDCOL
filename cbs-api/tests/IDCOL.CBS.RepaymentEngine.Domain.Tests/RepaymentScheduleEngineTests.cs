using IDCOL.CBS.RepaymentEngine.Domain;
using Xunit;

namespace IDCOL.CBS.RepaymentEngine.Domain.Tests;

public class RepaymentScheduleEngineTests
{
    [Fact]
    public void Generate_LevelPrincipal_AmortizesToZeroWithEqualPrincipal()
    {
        // 4 quarterly installments, no grace, level principal on a single 100k disbursement.
        var p = new ScheduleParameters
        {
            LoanAmount = 100_000,
            InterestRate = 0.10,
            DayCountBasis = 360,
            NumInstallments = 4,
            PrincipalType = "Level Principal",
            PaymentFrequency = 4,
            Disbursements = new List<Disbursement> { new() { DateSerial = 1000, Amount = 100_000 } },
            RepaymentDates = new List<double> { 1090, 1180, 1270, 1360 },
        };

        var rows = RepaymentScheduleEngine.Generate(p);

        Assert.Equal(4, rows.Count);
        // Level principal: each period repays 25,000 of the 100,000 principal.
        Assert.All(rows, r => Assert.Equal(25_000, r.Principal, 4));
        // Fully amortized: final closing balance is zero.
        Assert.Equal(0, rows[^1].ClosingBal, 4);
        // Reconciliation invariant on the first row: closing = opening + capInterest - principal.
        Assert.Equal(rows[0].OpeningBal + rows[0].CapInterest - rows[0].Principal, rows[0].ClosingBal, 4);
    }

    [Fact]
    public void Generate_NoDisbursements_ReturnsEmptySchedule()
    {
        var p = new ScheduleParameters
        {
            LoanAmount = 100_000,
            NumInstallments = 4,
            RepaymentDates = new List<double> { 1090, 1180 },
        };

        Assert.Empty(RepaymentScheduleEngine.Generate(p));
    }

    [Fact]
    public void Generate_PrincipalGrace_DefersPrincipalButAccruesInterest()
    {
        // Principal grace through the first payment date: row 0 has zero principal but non-zero interest.
        var p = new ScheduleParameters
        {
            LoanAmount = 100_000,
            InterestRate = 0.10,
            DayCountBasis = 360,
            NumInstallments = 3,
            PrincipalType = "Level Principal",
            PaymentFrequency = 4,
            PrincipalGracePeriodEnd = 1090,
            Disbursements = new List<Disbursement> { new() { DateSerial = 1000, Amount = 100_000 } },
            RepaymentDates = new List<double> { 1090, 1180, 1270, 1360 },
        };

        var rows = RepaymentScheduleEngine.Generate(p);

        Assert.Equal(0, rows[0].Principal, 4);
        Assert.True(rows[0].Interest > 0);
        Assert.Equal(0, rows[^1].ClosingBal, 4);
    }
}
