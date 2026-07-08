using IDCOL.CBS.Disbursement.Domain;
using Xunit;

namespace IDCOL.CBS.LoanLifecycle.Domain.Tests;

public class DisbursementRequestTests
{
    private static DisbursementRequest Initiate(string buUser = "bu1") => DisbursementRequest.Initiate(
        Guid.NewGuid(), "DS-20260706-abc123", 1, Guid.NewGuid(), "SANC-2026-001", "C-001",
        "GHEL Solar SIP", "BDT", 10_000_000m, 0m, "First tranche", buUser);

    private static readonly List<(string, string, decimal, decimal)> BalancedGl = new()
    {
        ("102030", "Loan Account", 10_000_000m, 0m),
        ("202030", "Bank Account", 0m, 10_000_000m),
    };

    [Fact]
    public void FullFlow_WithThreeDistinctUsers_ReachesProcessed()
    {
        var d = Initiate("bu1");

        Assert.True(d.Propose("cad1", 10_000_000m, 0m, "ok").IsSuccess);
        Assert.Equal("Proposed", d.Status);

        Assert.True(d.Post("acct1", "EFT", new DateOnly(2026, 7, 6), BalancedGl).IsSuccess);
        Assert.Equal("Processed", d.Status);
        Assert.Equal(2, d.GlLines.Count);
    }

    [Fact]
    public void Propose_BySameUserAsInitiator_FailsStructural()
    {
        var d = Initiate("bu1");

        var result = d.Propose("bu1", 10_000_000m, 0m, null);

        Assert.True(result.IsFailure);
        Assert.Contains("STRUCTURAL_VIOLATION", result.Error);
        Assert.Equal("Suggested", d.Status);
    }

    [Fact]
    public void Post_BySameUserAsReviewer_FailsStructural()
    {
        var d = Initiate("bu1");
        d.Propose("cad1", 10_000_000m, 0m, null);

        var result = d.Post("cad1", "EFT", new DateOnly(2026, 7, 6), BalancedGl);

        Assert.True(result.IsFailure);
        Assert.Contains("STRUCTURAL_VIOLATION", result.Error);
        Assert.Equal("Proposed", d.Status);
    }

    [Fact]
    public void Post_WithUnbalancedGl_Fails()
    {
        var d = Initiate("bu1");
        d.Propose("cad1", 10_000_000m, 0m, null);

        var unbalanced = new List<(string, string, decimal, decimal)>
        {
            ("102030", "Loan Account", 10_000_000m, 0m),
            ("202030", "Bank Account", 0m, 9_000_000m),
        };

        var result = d.Post("acct1", "EFT", new DateOnly(2026, 7, 6), unbalanced);

        Assert.True(result.IsFailure);
        Assert.Contains("not balanced", result.Error);
    }

    [Fact]
    public void Post_BeforeReview_Fails()
    {
        var d = Initiate("bu1");

        var result = d.Post("acct1", "EFT", new DateOnly(2026, 7, 6), BalancedGl);

        Assert.True(result.IsFailure);
    }
}
