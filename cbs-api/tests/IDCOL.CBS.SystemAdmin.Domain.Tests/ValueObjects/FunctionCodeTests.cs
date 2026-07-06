using IDCOL.CBS.SystemAdmin.Domain.ValueObjects;
using Xunit;

namespace IDCOL.CBS.SystemAdmin.Domain.Tests.ValueObjects;

public class FunctionCodeTests
{
    [Fact]
    public void Of_NormalizesToUppercaseAndTrims()
    {
        var code = FunctionCode.Of("  disbursement_post  ");

        Assert.Equal("DISBURSEMENT_POST", code.Value);
    }

    [Fact]
    public void Of_EmptyValue_Throws()
    {
        Assert.Throws<ArgumentException>(() => FunctionCode.Of(""));
    }

    [Fact]
    public void Equality_IsCaseAndWhitespaceNormalized()
    {
        var a = FunctionCode.Of("disbursement_post");
        var b = FunctionCode.Of(" DISBURSEMENT_POST ");

        Assert.Equal(a, b);
    }
}
