using IDCOL.CBS.SharedKernel.Security;
using Xunit;

namespace IDCOL.CBS.SharedKernel.Tests.Security;

public class SequentialApprovalWorkflowTests
{
    private static SequentialApprovalWorkflow CreateThreeStageWorkflow() => new(new[]
    {
        new WorkflowStage(0, "Initiate", "BU"),
        new WorkflowStage(1, "Review", "CAD"),
        new WorkflowStage(2, "Post", "ACCOUNTS"),
    });

    [Fact]
    public void AdvanceStage_WithCorrectRoleAndDistinctUsers_CompletesWorkflow()
    {
        var workflow = CreateThreeStageWorkflow();

        Assert.True(workflow.AdvanceStage("bu-user", "BU").IsSuccess);
        Assert.True(workflow.AdvanceStage("cad-user", "CAD").IsSuccess);
        Assert.True(workflow.AdvanceStage("accounts-user", "ACCOUNTS").IsSuccess);

        Assert.True(workflow.IsComplete);
    }

    [Fact]
    public void AdvanceStage_WithWrongRole_Fails()
    {
        var workflow = CreateThreeStageWorkflow();

        var result = workflow.AdvanceStage("cad-user", "CAD");

        Assert.True(result.IsFailure);
        Assert.Equal(0, workflow.CurrentStageIndex);
    }

    [Fact]
    public void AdvanceStage_BySameUserTwice_FailsWithStructuralViolation()
    {
        var workflow = CreateThreeStageWorkflow();
        workflow.AdvanceStage("same-user", "BU");

        // Same user somehow also holds the CAD role - still must not be allowed to
        // perform a second stage of the same request.
        var result = workflow.AdvanceStage("same-user", "CAD");

        Assert.True(result.IsFailure);
        Assert.Contains("STRUCTURAL_VIOLATION", result.Error);
    }

    [Fact]
    public void AdvanceStage_AfterComplete_Fails()
    {
        var workflow = CreateThreeStageWorkflow();
        workflow.AdvanceStage("bu-user", "BU");
        workflow.AdvanceStage("cad-user", "CAD");
        workflow.AdvanceStage("accounts-user", "ACCOUNTS");

        var result = workflow.AdvanceStage("someone-else", "ACCOUNTS");

        Assert.True(result.IsFailure);
    }
}
