using IDCOL.CBS.SharedKernel.Domain;
using IDCOL.CBS.SystemAdmin.Domain.ValueObjects;

namespace IDCOL.CBS.SystemAdmin.Domain.Entities;

/// <summary>
/// One row per (user, function). A user can be assigned as Maker OR Checker for a given
/// function, never both - this is the first of the three layers of structural maker-checker
/// enforcement described in the architecture plan (the other two are the application-pipeline
/// MakerCheckerGateBehavior and a database CHECK/UNIQUE constraint on this exact shape).
/// </summary>
public class RoleAssignment : Entity<Guid>
{
    public Guid UserId { get; private set; }

    public FunctionCode FunctionCode { get; private set; } = default!;

    public bool IsMaker { get; private set; }

    public bool IsChecker { get; private set; }

    public string AssignedBy { get; private set; } = default!;

    public DateTime AssignedAtUtc { get; private set; }

    private RoleAssignment()
    {
    }

    internal static RoleAssignment Create(
        Guid id, Guid userId, FunctionCode functionCode, bool isMaker, bool isChecker, string assignedBy)
    {
        if (isMaker && isChecker)
            throw new InvalidOperationException(
                "STRUCTURAL_VIOLATION: a role assignment cannot be both Maker and Checker for the same function.");
        if (!isMaker && !isChecker)
            throw new InvalidOperationException("A role assignment must be either Maker or Checker.");
        if (string.IsNullOrWhiteSpace(assignedBy))
            throw new ArgumentException("Assigned-by user id is required.", nameof(assignedBy));

        return new RoleAssignment
        {
            Id = id,
            UserId = userId,
            FunctionCode = functionCode,
            IsMaker = isMaker,
            IsChecker = isChecker,
            AssignedBy = assignedBy,
            AssignedAtUtc = DateTime.UtcNow
        };
    }
}
