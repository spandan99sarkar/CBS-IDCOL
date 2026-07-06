namespace IDCOL.CBS.BuildingBlocks.Application.Abstractions;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Marker interface for commands that must be committed atomically via a single
/// IUnitOfWork.SaveChangesAsync call after the handler runs. Queries never implement this.
/// </summary>
public interface ITransactionalCommand
{
}
