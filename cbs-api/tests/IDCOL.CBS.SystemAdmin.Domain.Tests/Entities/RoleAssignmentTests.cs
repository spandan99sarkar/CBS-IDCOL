using IDCOL.CBS.SystemAdmin.Domain.Entities;
using IDCOL.CBS.SystemAdmin.Domain.ValueObjects;
using Xunit;

namespace IDCOL.CBS.SystemAdmin.Domain.Tests.Entities;

/// <summary>
/// Exercises RoleAssignment.Create directly (via InternalsVisibleTo) to prove the invariant is
/// enforced by the entity itself, not only by User.AssignRole's pre-check - this is what makes
/// it a real second line of defense rather than duplicated logic in the aggregate root alone.
/// </summary>
public class RoleAssignmentTests
{
    [Fact]
    public void Create_AsMakerOnly_Succeeds()
    {
        var assignment = RoleAssignment.Create(
            Guid.NewGuid(), Guid.NewGuid(), FunctionCode.Of("DISBURSEMENT_POST"), isMaker: true, isChecker: false, "admin");

        Assert.True(assignment.IsMaker);
        Assert.False(assignment.IsChecker);
    }

    [Fact]
    public void Create_AsBothMakerAndChecker_ThrowsStructuralViolation()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => RoleAssignment.Create(
            Guid.NewGuid(), Guid.NewGuid(), FunctionCode.Of("DISBURSEMENT_POST"), isMaker: true, isChecker: true, "admin"));

        Assert.Contains("STRUCTURAL_VIOLATION", ex.Message);
    }

    [Fact]
    public void Create_AsNeitherMakerNorChecker_Throws()
    {
        Assert.Throws<InvalidOperationException>(() => RoleAssignment.Create(
            Guid.NewGuid(), Guid.NewGuid(), FunctionCode.Of("DISBURSEMENT_POST"), isMaker: false, isChecker: false, "admin"));
    }
}
