using System.Text.Json;
using FluentValidation;
using IDCOL.CBS.BuildingBlocks.Application.Abstractions;
using IDCOL.CBS.RepaymentEngine.Application.Abstractions;
using IDCOL.CBS.RepaymentEngine.Domain;
using MediatR;

namespace IDCOL.CBS.RepaymentEngine.Application.Commands;

/// <summary>
/// Adds a new version to a Facility - a reschedule, restructure, rate change, prepayment, or
/// moratorium extension. Requires the CAD role (this is where "the repayment schedule is
/// recalculated on any rate/term/principal/date change" happens) and supersedes the current
/// version rather than mutating it, so the full history stays intact.
/// </summary>
public sealed record AddFacilityVersionCommand(
    Guid FacilityId,
    FacilityVersionEventType EventType,
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
    ScheduleParameters Parameters) : IRequest<Guid>, IAuditableAction, ITransactionalCommand;

public sealed class AddFacilityVersionCommandValidator : AbstractValidator<AddFacilityVersionCommand>
{
    public AddFacilityVersionCommandValidator()
    {
        RuleFor(x => x.FacilityId).NotEmpty();
        RuleFor(x => x.Label).NotEmpty();
        RuleFor(x => x.Parameters.NumInstallments).GreaterThan(0);
        RuleFor(x => x.Parameters.Disbursements).NotEmpty();
        RuleFor(x => x.CapitalizedAmount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.WaivedAmount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.OverdueAmountRolledIn).GreaterThanOrEqualTo(0);
    }
}

public sealed class AddFacilityVersionCommandHandler : IRequestHandler<AddFacilityVersionCommand, Guid>
{
    private readonly IFacilityRepository _repository;
    private readonly ICurrentUserAccessor _currentUser;

    public AddFacilityVersionCommandHandler(IFacilityRepository repository, ICurrentUserAccessor currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(AddFacilityVersionCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsInRole("CAD"))
            throw new UnauthorizedAccessException("Only a CAD user can reschedule/restructure a facility.");

        var facility = await _repository.GetByIdAsync(request.FacilityId, cancellationToken)
            ?? throw new InvalidOperationException("Facility not found.");

        var newVersionId = Guid.NewGuid();
        var result = facility.AddVersion(
            newVersionId, request.EventType, request.EffectiveDate, request.Label, request.SourceFile,
            request.RateBeforePercent, request.RateAfterPercent, request.TenorMonthsBefore, request.TenorMonthsAfter,
            request.CapitalizedAmount, request.WaivedAmount, request.OverdueAmountRolledIn, request.RegulatoryReference,
            JsonSerializer.Serialize(request.Parameters), _currentUser.UserId);

        if (result.IsFailure)
            throw new InvalidOperationException(result.Error);

        return newVersionId;
    }
}
