using IDCOL.CBS.SharedKernel.Security;
using Xunit;

namespace IDCOL.CBS.SharedKernel.Tests.Security;

public class MakerCheckerRequestTests
{
    [Fact]
    public void Approve_ByDifferentUser_Succeeds()
    {
        var request = TestMakerCheckerRequest.Create("payload", "maker-1");

        var result = request.Approve("checker-1");

        Assert.True(result.IsSuccess);
        Assert.Equal(MakerCheckerStatus.Approved, request.Status);
        Assert.Equal("checker-1", request.CheckerUserId);
    }

    [Fact]
    public void Approve_BySameUserAsMaker_FailsWithStructuralViolation()
    {
        var request = TestMakerCheckerRequest.Create("payload", "maker-1");

        var result = request.Approve("maker-1");

        Assert.True(result.IsFailure);
        Assert.Contains("STRUCTURAL_VIOLATION", result.Error);
        Assert.Equal(MakerCheckerStatus.Pending, request.Status);
    }

    [Fact]
    public void Approve_BySameUserDifferentCasing_StillFailsWithStructuralViolation()
    {
        var request = TestMakerCheckerRequest.Create("payload", "Maker-1");

        var result = request.Approve("maker-1");

        Assert.True(result.IsFailure);
        Assert.Contains("STRUCTURAL_VIOLATION", result.Error);
    }

    [Fact]
    public void Reject_BySameUserAsMaker_FailsWithStructuralViolation()
    {
        var request = TestMakerCheckerRequest.Create("payload", "maker-1");

        var result = request.Reject("maker-1", "trying to reject my own request");

        Assert.True(result.IsFailure);
        Assert.Contains("STRUCTURAL_VIOLATION", result.Error);
    }

    [Fact]
    public void Reject_WithoutComment_Fails()
    {
        var request = TestMakerCheckerRequest.Create("payload", "maker-1");

        var result = request.Reject("checker-1", "");

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Approve_WhenAlreadyApproved_Fails()
    {
        var request = TestMakerCheckerRequest.Create("payload", "maker-1");
        request.Approve("checker-1");

        var result = request.Approve("checker-2");

        Assert.True(result.IsFailure);
    }
}
