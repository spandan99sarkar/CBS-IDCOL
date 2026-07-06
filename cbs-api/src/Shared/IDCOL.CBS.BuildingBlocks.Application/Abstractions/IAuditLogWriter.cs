namespace IDCOL.CBS.BuildingBlocks.Application.Abstractions;

public interface IAuditLogWriter
{
    Task WriteAsync(AuditLogEntryData entry, CancellationToken cancellationToken = default);
}

public sealed record AuditLogEntryData(
    string ActorUserId,
    string ActionName,
    string? EntityType,
    string? EntityId,
    string? DetailsJson,
    DateTime OccurredAtUtc);
