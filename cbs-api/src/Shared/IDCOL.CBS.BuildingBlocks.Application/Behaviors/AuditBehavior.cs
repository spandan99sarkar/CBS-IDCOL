using System.Text.Json;
using IDCOL.CBS.BuildingBlocks.Application.Abstractions;
using MediatR;

namespace IDCOL.CBS.BuildingBlocks.Application.Behaviors;

/// <summary>
/// Writes an audit-trail row for every command implementing IAuditableAction (Bangladesh Bank
/// ICT Security Guideline: transaction types, overrides, and parameter changes must be logged
/// with actor identity and timestamp). Queries are not audited to keep the log to writes only.
/// </summary>
public sealed class AuditBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ICurrentUserAccessor _currentUser;
    private readonly IAuditLogWriter _auditLogWriter;

    public AuditBehavior(ICurrentUserAccessor currentUser, IAuditLogWriter auditLogWriter)
    {
        _currentUser = currentUser;
        _auditLogWriter = auditLogWriter;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is not IAuditableAction)
            return await next();

        var response = await next();

        await _auditLogWriter.WriteAsync(
            new AuditLogEntryData(
                ActorUserId: _currentUser.UserId,
                ActionName: typeof(TRequest).Name,
                EntityType: null,
                EntityId: null,
                DetailsJson: JsonSerializer.Serialize(request),
                OccurredAtUtc: DateTime.UtcNow),
            cancellationToken);

        return response;
    }
}
