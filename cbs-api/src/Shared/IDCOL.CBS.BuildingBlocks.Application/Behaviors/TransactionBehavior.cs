using IDCOL.CBS.BuildingBlocks.Application.Abstractions;
using MediatR;

namespace IDCOL.CBS.BuildingBlocks.Application.Behaviors;

/// <summary>
/// Wraps command handlers that implement ITransactionalCommand in a single SaveChanges call so
/// a handler's writes commit atomically. Queries and non-transactional commands skip this.
/// </summary>
public sealed class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IUnitOfWork _unitOfWork;

    public TransactionBehavior(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is not ITransactionalCommand)
            return await next();

        var response = await next();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return response;
    }
}
