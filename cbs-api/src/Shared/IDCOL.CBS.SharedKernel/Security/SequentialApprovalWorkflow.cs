using IDCOL.CBS.SharedKernel.Common;

namespace IDCOL.CBS.SharedKernel.Security;

/// <summary>
/// One stage of a multi-party approval chain, e.g. Disbursement's
/// Suggested(BU) -> Proposed(CAD) -> Processed(Accounts), or CreditSanction's
/// BU -> RM -> BU Head -> Dept Head -> CAD -> Board chain.
/// </summary>
public sealed record WorkflowStage(int StageIndex, string StageName, string RequiredRoleCode);

/// <summary>
/// Reusable, embeddable multi-stage workflow shape shared by every bounded context that needs
/// more than a single maker/checker pair. Structurally enforces that no single user can complete
/// more than one stage of the same request, on top of each stage's required-role check.
/// </summary>
public sealed class SequentialApprovalWorkflow
{
    private readonly List<string> _completedByUserIds = new();

    public IReadOnlyList<WorkflowStage> Stages { get; }

    public int CurrentStageIndex { get; private set; }

    public bool IsComplete => CurrentStageIndex >= Stages.Count;

    public WorkflowStage? CurrentStage => IsComplete ? null : Stages[CurrentStageIndex];

    public SequentialApprovalWorkflow(IReadOnlyList<WorkflowStage> stages)
    {
        if (stages is null || stages.Count == 0)
            throw new ArgumentException("A workflow must have at least one stage.", nameof(stages));

        Stages = stages;
        CurrentStageIndex = 0;
    }

    public Result AdvanceStage(string actingUserId, string actingUserRoleCode)
    {
        if (IsComplete)
            return Result.Fail("Workflow is already complete.");

        var stage = Stages[CurrentStageIndex];
        if (!string.Equals(stage.RequiredRoleCode, actingUserRoleCode, StringComparison.OrdinalIgnoreCase))
            return Result.Fail($"Stage '{stage.StageName}' requires role '{stage.RequiredRoleCode}'.");

        if (_completedByUserIds.Contains(actingUserId, StringComparer.OrdinalIgnoreCase))
            return Result.Fail(
                "STRUCTURAL_VIOLATION: a user cannot perform more than one stage of the same request.");

        _completedByUserIds.Add(actingUserId);
        CurrentStageIndex++;
        return Result.Success();
    }
}
