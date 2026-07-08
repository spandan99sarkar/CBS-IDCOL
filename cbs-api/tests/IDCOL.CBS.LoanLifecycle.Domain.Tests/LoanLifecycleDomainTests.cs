using IDCOL.CBS.CreditSanction.Domain;
using IDCOL.CBS.PartyKyc.Domain;
using IDCOL.CBS.ProductConfig.Domain;
using Xunit;

namespace IDCOL.CBS.LoanLifecycle.Domain.Tests;

public class LoanLifecycleDomainTests
{
    [Fact]
    public void LoanProduct_Create_NormalizesCodeAndDefaultsActive()
    {
        var p = LoanProduct.Create(
            Guid.NewGuid(), " tl-infra ", "Infrastructure Term Loan", "Term Loan", "Fixed",
            "Level Principal", 360, 12, true, true, 9m, 8m, 12m, "admin");

        Assert.Equal("TL-INFRA", p.ProductCode);
        Assert.True(p.IsActive);
    }

    [Fact]
    public void LoanProduct_Create_LowerRateAboveUpper_Throws()
    {
        Assert.Throws<ArgumentException>(() => LoanProduct.Create(
            Guid.NewGuid(), "X", "X", "Term Loan", "Fixed", "Level Principal", 360, 0, true, true,
            9m, 12m, 8m, "admin"));
    }

    [Fact]
    public void Customer_Create_DefaultsAndTrims()
    {
        var c = Customer.Create(
            Guid.NewGuid(), " C-001 ", "Institutional", " Acme Ltd ", "IF",
            null, null, null, "", "", "", "admin");

        Assert.Equal("C-001", c.CustomerNo);
        Assert.Equal("Acme Ltd", c.Name);
        Assert.Equal("Pending", c.KycStatus);
        Assert.Equal("Low", c.RiskLevel);
        Assert.Equal("Local", c.Source);
    }

    [Fact]
    public void LoanAgreement_Sign_FromDraft_Succeeds()
    {
        var a = CreateAgreement();

        var result = a.Sign("checker");

        Assert.True(result.IsSuccess);
        Assert.Equal("Signed", a.Status);
    }

    [Fact]
    public void LoanAgreement_Sign_Twice_FailsSecondTime()
    {
        var a = CreateAgreement();
        a.Sign("checker");

        var result = a.Sign("checker");

        Assert.True(result.IsFailure);
    }

    private static LoanAgreement CreateAgreement() => LoanAgreement.Create(
        Guid.NewGuid(), "SANC-2026-001", Guid.NewGuid(), "C-001", "TL-INFRA", "Solar Park", "Power",
        "BDT", 100_000_000m, "BDT", 0m, new DateOnly(2026, 1, 15), new DateOnly(2027, 1, 15),
        "Fixed", 9m, 84, 24, 0, 12, "Level Principal", 4, 4, 360, 2m, "A+", "admin");
}
