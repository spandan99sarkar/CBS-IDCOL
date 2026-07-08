using IDCOL.CBS.SystemAdmin.Application.Audit.Queries.GetAuditTrail;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IDCOL.CBS.Api.Controllers;

[ApiController]
[Route("api/audit")]
[Authorize]
public class AuditController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuditController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetRecent(
        [FromQuery] int take = 100, CancellationToken cancellationToken = default)
    {
        var entries = await _mediator.Send(new GetAuditTrailQuery(take), cancellationToken);
        return Ok(entries);
    }
}
