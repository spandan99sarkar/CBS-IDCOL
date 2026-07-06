using IDCOL.CBS.SystemAdmin.Application.Abstractions;
using MediatR;

namespace IDCOL.CBS.SystemAdmin.Application.Audit.Queries.GetAuditTrail;

public sealed record GetAuditTrailQuery(int Take = 100) : IRequest<IReadOnlyList<AuditLogEntryDto>>;

public sealed class GetAuditTrailQueryHandler : IRequestHandler<GetAuditTrailQuery, IReadOnlyList<AuditLogEntryDto>>
{
    private readonly IAuditTrailReader _auditTrailReader;

    public GetAuditTrailQueryHandler(IAuditTrailReader auditTrailReader) => _auditTrailReader = auditTrailReader;

    public Task<IReadOnlyList<AuditLogEntryDto>> Handle(GetAuditTrailQuery request, CancellationToken cancellationToken) =>
        _auditTrailReader.GetRecentAsync(request.Take, cancellationToken);
}
