namespace IDCOL.CBS.SystemAdmin.Application.Abstractions;

public interface IAuditTrailReader
{
    Task<IReadOnlyList<AuditLogEntryDto>> GetRecentAsync(int take, CancellationToken cancellationToken = default);
}

public sealed record AuditLogEntryDto(
    Guid Id,
    string ActorUserId,
    string ActionName,
    string? EntityType,
    string? EntityId,
    DateTime OccurredAtUtc);
