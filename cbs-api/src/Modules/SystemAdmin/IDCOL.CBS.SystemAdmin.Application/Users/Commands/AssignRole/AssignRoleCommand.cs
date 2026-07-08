using IDCOL.CBS.BuildingBlocks.Application.Abstractions;
using MediatR;

namespace IDCOL.CBS.SystemAdmin.Application.Users.Commands.AssignRole;

/// <summary>
/// Assigns a user as Maker XOR Checker for a given function code. This is a foundational
/// admin operation (bootstrapping the maker-checker matrix itself), so it is audited and
/// transactional but not itself maker-checker-gated - see the architecture plan's note that
/// SystemAdmin's own ParameterChangeRequest workflow is a separate, later concern.
/// </summary>
public sealed record AssignRoleCommand(
    Guid UserId,
    string FunctionCode,
    bool IsMaker,
    bool IsChecker) : IRequest, IAuditableAction, ITransactionalCommand;
