namespace IDCOL.CBS.BuildingBlocks.Application.Abstractions;

/// <summary>
/// Marker interface for any command whose execution must produce an audit-trail row
/// (Bangladesh Bank ICT Security Guideline: every transaction type, override, and
/// parameter change must be traceable to an actor and a timestamp).
/// </summary>
public interface IAuditableAction
{
}
