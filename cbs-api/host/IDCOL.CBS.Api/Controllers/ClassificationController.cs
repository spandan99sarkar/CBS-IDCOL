using IDCOL.CBS.Classification.Application.Commands;
using IDCOL.CBS.Classification.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IDCOL.CBS.Api.Controllers;

[ApiController]
[Route("api/classification")]
[Authorize]
public class ClassificationController : ControllerBase
{
    private readonly IMediator _mediator;

    public ClassificationController(IMediator mediator) => _mediator = mediator;

    /// <summary>Latest classification run's results (the CL statement / dashboard).</summary>
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken) =>
        Ok(await _mediator.Send(new ListClassificationsQuery(), cancellationToken));

    [HttpPost("run")]
    public async Task<IActionResult> Run([FromBody] RunClassificationCommand command, CancellationToken cancellationToken)
    {
        var runId = await _mediator.Send(command, cancellationToken);
        return Ok(new { runId });
    }
}
