namespace IDCOL.CBS.BuildingBlocks.Application.Abstractions;

/// <summary>
/// Implemented in SystemAdmin.Infrastructure. Deliberately owned here (BuildingBlocks, not
/// SystemAdmin.Application) so every module's pipeline can depend on this abstraction without
/// taking a project reference on the SystemAdmin bounded context.
/// </summary>
public interface IMakerCheckerRoleGate
{
    Task<bool> CanActAsCheckerAsync(string userId, string functionCode, CancellationToken cancellationToken = default);
}
