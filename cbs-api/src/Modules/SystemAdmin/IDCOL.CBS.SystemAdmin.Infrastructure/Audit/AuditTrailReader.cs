using IDCOL.CBS.SystemAdmin.Application.Abstractions;
using IDCOL.CBS.SystemAdmin.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IDCOL.CBS.SystemAdmin.Infrastructure.Audit;

public class AuditTrailReader : IAuditTrailReader
{
    private readonly SystemAdminDbContext _dbContext;

    public AuditTrailReader(SystemAdminDbContext dbContext) => _dbContext = dbContext;

    public async Task<IReadOnlyList<AuditLogEntryDto>> GetRecentAsync(
        int take, CancellationToken cancellationToken = default) =>
        await _dbContext.AuditLogEntries
            .OrderByDescending(a => a.OccurredAtUtc)
            .Take(take)
            .Select(a => new AuditLogEntryDto(
                a.Id, a.ActorUserId, a.ActionName, a.EntityType, a.EntityId, a.OccurredAtUtc))
            .ToListAsync(cancellationToken);
}
