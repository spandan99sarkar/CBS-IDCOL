using IDCOL.CBS.Collection.Domain;
using Xunit;

namespace IDCOL.CBS.LoanLifecycle.Domain.Tests;

public class CollectionTests
{
    [Fact]
    public void Waterfall_ClearsLpcThenInterestThenPrincipal()
    {
        // 10,000 received against dues of LPC 500, Interest 3,000, Principal 20,000.
        var a = PaymentWaterfall.Allocate(received: 10_000m, dueLpc: 500m, dueInterest: 3_000m, duePrincipal: 20_000m);

        Assert.Equal(500m, a.Lpc);
        Assert.Equal(3_000m, a.Interest);
        Assert.Equal(6_500m, a.Principal); // remainder
        Assert.Equal(10_000m, a.Total);
    }

    [Fact]
    public void Waterfall_Overpayment_AllExcessToPrincipal()
    {
        // Received exceeds all dues; the excess reduces principal (prepayment).
        var a = PaymentWaterfall.Allocate(received: 30_000m, dueLpc: 500m, dueInterest: 3_000m, duePrincipal: 20_000m);

        Assert.Equal(500m, a.Lpc);
        Assert.Equal(3_000m, a.Interest);
        Assert.Equal(26_500m, a.Principal);
    }

    private static Receipt EnterReceipt(string cadUser = "cad1") => Receipt.Enter(
        Guid.NewGuid(), "CO-20260706-abc123", Guid.NewGuid(), "SANC-100", "C-100", "Kazi Solar",
        "BDT", "EFT", "TXN-1", "Janata Bank", 10_000m,
        new DateOnly(2026, 7, 6), new DateOnly(2026, 7, 6), null,
        principalAmount: 6_500m, interestAmount: 3_000m, lpcAmount: 500m, cadUser);

    [Fact]
    public void Enter_AllocationNotSummingToInstrument_Throws()
    {
        Assert.Throws<ArgumentException>(() => Receipt.Enter(
            Guid.NewGuid(), "CO-1", Guid.NewGuid(), "SANC-100", "C-100", "Kazi", "BDT", "EFT", null, null,
            10_000m, new DateOnly(2026, 7, 6), new DateOnly(2026, 7, 6), null,
            principalAmount: 5_000m, interestAmount: 3_000m, lpcAmount: 500m, "cad1")); // sums to 8,500 != 10,000
    }

    [Fact]
    public void Verify_ByDifferentUser_PostsBalancedGl()
    {
        var r = EnterReceipt("cad1");
        var lines = new List<(string, string, decimal, decimal)>
        {
            ("202030", "Bank", 10_000m, 0m),
            ("102030", "Loan Principal", 0m, 6_500m),
            ("401010", "Interest Income", 0m, 3_000m),
            ("401020", "LPC Income", 0m, 500m),
        };

        var result = r.Verify("acct1", "reconciled", lines);

        Assert.True(result.IsSuccess);
        Assert.Equal("Verified", r.Status);
        Assert.Equal(4, r.GlLines.Count);
    }

    [Fact]
    public void Verify_BySameUserAsEnterer_FailsStructural()
    {
        var r = EnterReceipt("cad1");
        var lines = new List<(string, string, decimal, decimal)> { ("202030", "Bank", 10_000m, 0m), ("102030", "P", 0m, 10_000m) };

        var result = r.Verify("cad1", null, lines);

        Assert.True(result.IsFailure);
        Assert.Contains("STRUCTURAL_VIOLATION", result.Error);
        Assert.Equal("Pending", r.Status);
    }
}
