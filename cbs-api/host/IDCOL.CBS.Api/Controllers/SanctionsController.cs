using IDCOL.CBS.CreditSanction.Application.Sanctions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IDCOL.CBS.Api.Controllers;

[ApiController]
[Route("api/sanctions")]
[Authorize]
public class SanctionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public SanctionsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken) =>
        Ok(await _mediator.Send(new ListSanctionsQuery(), cancellationToken));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSanctionCommand command, CancellationToken cancellationToken)
    {
        var id = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(Create), new { id }, new { id });
    }

    [HttpPost("{sanctionId:guid}/sign")]
    public async Task<IActionResult> Sign(Guid sanctionId, CancellationToken cancellationToken)
    {
        await _mediator.Send(new SignSanctionCommand(sanctionId), cancellationToken);
        return NoContent();
    }
}
