using IDCOL.CBS.RepaymentEngine.Domain;
using MediatR;

namespace IDCOL.CBS.RepaymentEngine.Application.ComputeSchedule;

/// <summary>
/// Computes a repayment schedule from loan parameters. A query (read/compute, no persistence),
/// so it is not audited or transaction-wrapped - it is a pure calculation over the request.
/// </summary>
public sealed record ComputeScheduleQuery(ScheduleParameters Parameters) : IRequest<IReadOnlyList<ScheduleRow>>;

public sealed class ComputeScheduleQueryHandler
    : IRequestHandler<ComputeScheduleQuery, IReadOnlyList<ScheduleRow>>
{
    public Task<IReadOnlyList<ScheduleRow>> Handle(ComputeScheduleQuery request, CancellationToken cancellationToken)
    {
        IReadOnlyList<ScheduleRow> rows = RepaymentScheduleEngine.Generate(request.Parameters);
        return Task.FromResult(rows);
    }
}
