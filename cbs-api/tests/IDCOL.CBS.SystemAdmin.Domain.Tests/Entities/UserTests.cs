using IDCOL.CBS.SystemAdmin.Domain.Entities;
using IDCOL.CBS.SystemAdmin.Domain.ValueObjects;
using Xunit;

namespace IDCOL.CBS.SystemAdmin.Domain.Tests.Entities;

public class UserTests
{
    private static User CreateUser() => User.Create(
        Guid.NewGuid(), "jdoe", "Jane Doe", "jane@idcol.example", "hashed-password", "CAD", "system");

    [Fact]
    public void AssignRole_AsMakerOnly_Succeeds()
    {
        var user = CreateUser();

        var result = user.AssignRole(FunctionCode.Of("DISBURSEMENT_POST"), isMaker: true, isChecker: false, "admin");

        Assert.True(result.IsSuccess);
        var assignment = Assert.Single(user.RoleAssignments);
        Assert.True(assignment.IsMaker);
        Assert.False(assignment.IsChecker);
    }

    [Fact]
    public void AssignRole_AsBothMakerAndChecker_FailsWithStructuralViolation()
    {
        var user = CreateUser();

        var result = user.AssignRole(FunctionCode.Of("DISBURSEMENT_POST"), isMaker: true, isChecker: true, "admin");

        Assert.True(result.IsFailure);
        Assert.Contains("STRUCTURAL_VIOLATION", result.Error);
        Assert.Empty(user.RoleAssignments);
    }

    [Fact]
    public void AssignRole_TwiceForSameFunction_FailsOnSecondAttempt()
    {
        var user = CreateUser();
        user.AssignRole(FunctionCode.Of("DISBURSEMENT_POST"), isMaker: true, isChecker: false, "admin");

        var result = user.AssignRole(FunctionCode.Of("DISBURSEMENT_POST"), isMaker: false, isChecker: true, "admin");

        Assert.True(result.IsFailure);
        Assert.Single(user.RoleAssignments);
    }

    [Fact]
    public void AssignRole_ForDifferentFunctions_BothSucceed()
    {
        var user = CreateUser();

        var first = user.AssignRole(FunctionCode.Of("DISBURSEMENT_POST"), isMaker: true, isChecker: false, "admin");
        var second = user.AssignRole(FunctionCode.Of("PARAMETER_CHANGE"), isMaker: false, isChecker: true, "admin");

        Assert.True(first.IsSuccess);
        Assert.True(second.IsSuccess);
        Assert.Equal(2, user.RoleAssignments.Count);
    }

    [Fact]
    public void Deactivate_SetsIsActiveFalseAndRecordsModifier()
    {
        var user = CreateUser();

        user.Deactivate("admin");

        Assert.False(user.IsActive);
        Assert.Equal("admin", user.LastModifiedBy);
        Assert.NotNull(user.LastModifiedAtUtc);
    }

    [Fact]
    public void Create_WithoutPasswordHash_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            User.Create(Guid.NewGuid(), "jdoe", "Jane Doe", "jane@idcol.example", "", "CAD", "system"));
    }
}
