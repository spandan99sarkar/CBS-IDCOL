using IDCOL.CBS.BuildingBlocks.Application.Abstractions;

namespace IDCOL.CBS.SystemAdmin.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly SystemAdminDbContext _dbContext;

    public UnitOfWork(SystemAdminDbContext dbContext) => _dbContext = dbContext;

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _dbContext.SaveChangesAsync(cancellationToken);
}
