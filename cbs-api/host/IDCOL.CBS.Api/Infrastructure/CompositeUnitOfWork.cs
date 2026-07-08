using IDCOL.CBS.BuildingBlocks.Application.Abstractions;
using IDCOL.CBS.LoanLifecycle.Infrastructure.Persistence;
using IDCOL.CBS.SystemAdmin.Infrastructure.Persistence;

namespace IDCOL.CBS.Api.Infrastructure;

/// <summary>
/// As the modular monolith grows past a single DbContext, the transactional commit point must
/// cover every module's context. A given command only writes to one context, and SaveChanges on
/// an unchanged context is a no-op, so committing all of them per command is both correct and
/// cheap. New module DbContexts are added here as they come online.
/// </summary>
public sealed class CompositeUnitOfWork : IUnitOfWork
{
    private readonly SystemAdminDbContext _systemAdmin;
    private readonly LoanLifecycleDbContext _loanLifecycle;

    public CompositeUnitOfWork(SystemAdminDbContext systemAdmin, LoanLifecycleDbContext loanLifecycle)
    {
        _systemAdmin = systemAdmin;
        _loanLifecycle = loanLifecycle;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var affected = await _systemAdmin.SaveChangesAsync(cancellationToken);
        affected += await _loanLifecycle.SaveChangesAsync(cancellationToken);
        return affected;
    }
}
