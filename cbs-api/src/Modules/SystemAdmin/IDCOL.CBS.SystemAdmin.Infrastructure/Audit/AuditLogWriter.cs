using IDCOL.CBS.BuildingBlocks.Application.Abstractions;
using IDCOL.CBS.SystemAdmin.Domain.Entities;
using IDCOL.CBS.SystemAdmin.Infrastructure.Persistence;

namespace IDCOL.CBS.SystemAdmin.Infrastructure.Audit;

public class AuditLogWriter : IAuditLogWriter
{
    private readonly SystemAdminDbContext _dbContext;

    public AuditLogWriter(SystemAdminDbContext dbContext) => _dbContext = dbContext;

    public async Task WriteAsync(AuditLogEntryData entry, CancellationToken cancellationToken = default)
    {
        var logEntry = AuditLogEntry.Create(
            Guid.NewGuid(),
            entry.ActorUserId,
            entry.ActionName,
            entry.EntityType,
            entry.EntityId,
            entry.DetailsJson,
            entry.OccurredAtUtc);

        await _dbContext.AuditLogEntries.AddAsync(logEntry, cancellationToken);

        // Committed eagerly rather than relying on the command's own TransactionBehavior save,
        // so the audit row is durable even if that particular command isn't itself transactional.
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
