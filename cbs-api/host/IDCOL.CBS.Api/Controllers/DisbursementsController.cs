using IDCOL.CBS.Disbursement.Application.Commands;
using IDCOL.CBS.Disbursement.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IDCOL.CBS.Api.Controllers;

[ApiController]
[Route("api/disbursements")]
[Authorize]
public class DisbursementsController : ControllerBase
{
    private readonly IMediator _mediator;

    public DisbursementsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken) =>
        Ok(await _mediator.Send(new ListDisbursementsQuery(), cancellationToken));

    [HttpPost("initiate")]
    public async Task<IActionResult> Initiate(
        [FromBody] InitiateDisbursementCommand command, CancellationToken cancellationToken)
    {
        var id = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(Initiate), new { id }, new { id });
    }

    [HttpPost("{id:guid}/review")]
    public async Task<IActionResult> Review(
        Guid id, [FromBody] ReviewDisbursementRequest request, CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new ReviewDisbursementCommand(id, request.JustifiedLoanAmount, request.JustifiedGrantAmount, request.CadRemarks),
            cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/post")]
    public async Task<IActionResult> Post(
        Guid id, [FromBody] PostDisbursementRequest request, CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new PostDisbursementCommand(id, request.DisbursementMode, request.ValueDate, request.GlLines),
            cancellationToken);
        return NoContent();
    }
}

public sealed record ReviewDisbursementRequest(
    decimal JustifiedLoanAmount, decimal JustifiedGrantAmount, string? CadRemarks);

public sealed record PostDisbursementRequest(
    string DisbursementMode, DateOnly ValueDate, IReadOnlyList<GlLineInput> GlLines);
