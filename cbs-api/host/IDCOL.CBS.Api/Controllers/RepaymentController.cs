using IDCOL.CBS.RepaymentEngine.Application.ComputeSchedule;
using IDCOL.CBS.RepaymentEngine.Domain;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IDCOL.CBS.Api.Controllers;

[ApiController]
[Route("api/repayment")]
[Authorize]
public class RepaymentController : ControllerBase
{
    private readonly IMediator _mediator;

    public RepaymentController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Computes a repayment schedule from the supplied loan parameters. This is the engine
    /// endpoint the CAD "Advance Schedule Generation" screen calls on every parameter change.
    /// </summary>
    [HttpPost("compute")]
    public async Task<IActionResult> Compute(
        [FromBody] ScheduleParameters parameters, CancellationToken cancellationToken)
    {
        var rows = await _mediator.Send(new ComputeScheduleQuery(parameters), cancellationToken);
        return Ok(rows);
    }
}
