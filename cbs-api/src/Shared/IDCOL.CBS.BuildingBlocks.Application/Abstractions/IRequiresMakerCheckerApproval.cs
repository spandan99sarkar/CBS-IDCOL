namespace IDCOL.CBS.BuildingBlocks.Application.Abstractions;

/// <summary>
/// Implemented by any MediatR command whose execution must be gated by maker-checker role
/// segregation. This is the application-pipeline half of the three-layer structural enforcement
/// described in the architecture plan: the domain layer's MakerCheckerRequest.Approve() enforces
/// the same-user rule independently, and a database CHECK constraint on role assignment is the
/// third, defense-in-depth layer.
/// </summary>
public interface IRequiresMakerCheckerApproval
{
    string FunctionCode { get; }
}
