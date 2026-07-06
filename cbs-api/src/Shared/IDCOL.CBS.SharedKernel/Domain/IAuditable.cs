namespace IDCOL.CBS.SharedKernel.Domain;

/// <summary>
/// Implemented by any entity whose writes must appear in the system-wide audit trail
/// (Bangladesh Bank ICT Security Guideline: every create/modify must be traceable to a user and a timestamp).
/// </summary>
public interface IAuditable
{
    string CreatedBy { get; }
    DateTime CreatedAtUtc { get; }
    string? LastModifiedBy { get; }
    DateTime? LastModifiedAtUtc { get; }
}
