using IDCOL.CBS.BuildingBlocks.Application.Abstractions;
using MediatR;

namespace IDCOL.CBS.BuildingBlocks.Application.Behaviors;

/// <summary>
/// Application-layer half of the three-layer maker-checker enforcement. Short-circuits before
/// the handler runs if the current user does not hold the checker role for this function -
/// independent of the domain-layer MakerCheckerRequest.Approve() same-user check and the
/// database CHECK constraint on role assignment described in the architecture plan.
/// </summary>
public sealed class MakerCheckerGateBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ICurrentUserAccessor _currentUser;
    private readonly IMakerCheckerRoleGate _roleGate;

    public MakerCheckerGateBehavior(ICurrentUserAccessor currentUser, IMakerCheckerRoleGate roleGate)
    {
        _currentUser = currentUser;
        _roleGate = roleGate;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is not IRequiresMakerCheckerApproval gated)
            return await next();

        var canAct = await _roleGate.CanActAsCheckerAsync(_currentUser.UserId, gated.FunctionCode, cancellationToken);
        if (!canAct)
            throw new MakerCheckerGateException(
                $"User '{_currentUser.UserId}' does not hold the checker role for function '{gated.FunctionCode}'.");

        return await next();
    }
}

public sealed class MakerCheckerGateException : Exception
{
    public MakerCheckerGateException(string message) : base(message)
    {
    }
}
