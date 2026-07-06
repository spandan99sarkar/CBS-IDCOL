using IDCOL.CBS.SharedKernel.Domain;

namespace IDCOL.CBS.SystemAdmin.Domain.Entities;

/// <summary>
/// Immutable audit-trail row. Never updated or deleted once written (Bangladesh Bank ICT
/// Security Guideline: reversals/corrections must be new entries, not edits to history).
/// </summary>
public class AuditLogEntry : Entity<Guid>
{
    public string ActorUserId { get; private set; } = default!;

    public string ActionName { get; private set; } = default!;

    public string? EntityType { get; private set; }

    public string? EntityId { get; private set; }

    public string? DetailsJson { get; private set; }

    public DateTime OccurredAtUtc { get; private set; }

    private AuditLogEntry()
    {
    }

    public static AuditLogEntry Create(
        Guid id,
        string actorUserId,
        string actionName,
        string? entityType,
        string? entityId,
        string? detailsJson,
        DateTime occurredAtUtc)
    {
        return new AuditLogEntry
        {
            Id = id,
            ActorUserId = actorUserId,
            ActionName = actionName,
            EntityType = entityType,
            EntityId = entityId,
            DetailsJson = detailsJson,
            OccurredAtUtc = occurredAtUtc
        };
    }
}
