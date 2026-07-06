using IDCOL.CBS.Collection.Application.Commands;
using IDCOL.CBS.Collection.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IDCOL.CBS.Api.Controllers;

[ApiController]
[Route("api/collections")]
[Authorize]
public class CollectionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public CollectionsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken) =>
        Ok(await _mediator.Send(new ListReceiptsQuery(), cancellationToken));

    [HttpPost]
    public async Task<IActionResult> Enter([FromBody] EnterReceiptCommand command, CancellationToken cancellationToken)
    {
        var id = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(Enter), new { id }, new { id });
    }

    [HttpPost("{id:guid}/verify")]
    public async Task<IActionResult> Verify(
        Guid id, [FromBody] VerifyReceiptRequest request, CancellationToken cancellationToken)
    {
        await _mediator.Send(new VerifyReceiptCommand(id, request.Comment), cancellationToken);
        return NoContent();
    }
}

public sealed record VerifyReceiptRequest(string? Comment);
