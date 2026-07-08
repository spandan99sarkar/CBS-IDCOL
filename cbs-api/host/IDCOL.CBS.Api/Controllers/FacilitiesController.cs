using IDCOL.CBS.RepaymentEngine.Application.Commands;
using IDCOL.CBS.RepaymentEngine.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IDCOL.CBS.Api.Controllers;

[ApiController]
[Route("api/facilities")]
[Authorize]
public class FacilitiesController : ControllerBase
{
    private readonly IMediator _mediator;

    public FacilitiesController(IMediator mediator) => _mediator = mediator;

    /// <summary>Every facility (usually one, occasionally two for a co-financed project) for a sanction, with full version history.</summary>
    [HttpGet("by-sanction/{sanctionId:guid}")]
    public async Task<IActionResult> BySanction(Guid sanctionId, CancellationToken cancellationToken) =>
        Ok(await _mediator.Send(new GetFacilitiesBySanctionQuery(sanctionId), cancellationToken));

    [HttpPost("original")]
    public async Task<IActionResult> CreateOriginal(
        [FromBody] CreateOriginalFacilityCommand command, CancellationToken cancellationToken)
    {
        var id = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(BySanction), new { sanctionId = command.SanctionId }, new { id });
    }

    /// <summary>Reschedule / restructure / rate-change / prepayment / moratorium-extension: adds a new version.</summary>
    [HttpPost("{facilityId:guid}/versions")]
    public async Task<IActionResult> AddVersion(
        Guid facilityId, [FromBody] AddFacilityVersionRequest request, CancellationToken cancellationToken)
    {
        var command = new AddFacilityVersionCommand(
            facilityId, request.EventType, request.EffectiveDate, request.Label, request.SourceFile,
            request.RateBeforePercent, request.RateAfterPercent, request.TenorMonthsBefore, request.TenorMonthsAfter,
            request.CapitalizedAmount, request.WaivedAmount, request.OverdueAmountRolledIn,
            request.RegulatoryReference, request.Parameters);
        var versionId = await _mediator.Send(command, cancellationToken);
        return Ok(new { versionId });
    }

    [HttpGet("{facilityId:guid}/versions/{versionId:guid}/schedule")]
    public async Task<IActionResult> GetSchedule(Guid facilityId, Guid versionId, CancellationToken cancellationToken) =>
        Ok(await _mediator.Send(new GetFacilityVersionScheduleQuery(facilityId, versionId), cancellationToken));

    [HttpPatch("{facilityId:guid}/versions/{versionId:guid}/installments/{installmentIndex:int}")]
    public async Task<IActionResult> ApplyOverride(
        Guid facilityId, Guid versionId, int installmentIndex,
        [FromBody] ApplyInstallmentOverrideRequest request, CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new ApplyInstallmentOverrideCommand(
                facilityId, versionId, installmentIndex, request.InterestOverride,
                request.OpeningBalanceOverride, request.ClosingBalanceOverride),
            cancellationToken);
        return NoContent();
    }
}

public sealed record AddFacilityVersionRequest(
    IDCOL.CBS.RepaymentEngine.Domain.FacilityVersionEventType EventType,
    DateOnly EffectiveDate,
    string Label,
    string? SourceFile,
    decimal? RateBeforePercent,
    decimal? RateAfterPercent,
    int? TenorMonthsBefore,
    int? TenorMonthsAfter,
    decimal CapitalizedAmount,
    decimal WaivedAmount,
    decimal OverdueAmountRolledIn,
    string? RegulatoryReference,
    IDCOL.CBS.RepaymentEngine.Domain.ScheduleParameters Parameters);

public sealed record ApplyInstallmentOverrideRequest(
    decimal? InterestOverride, decimal? OpeningBalanceOverride, decimal? ClosingBalanceOverride);
