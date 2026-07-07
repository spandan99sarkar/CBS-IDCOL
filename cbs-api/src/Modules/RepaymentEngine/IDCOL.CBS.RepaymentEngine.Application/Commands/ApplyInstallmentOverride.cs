using IDCOL.CBS.BuildingBlocks.Application.Abstractions;
using IDCOL.CBS.RepaymentEngine.Application.Abstractions;
using MediatR;

namespace IDCOL.CBS.RepaymentEngine.Application.Commands;

/// <summary>
/// In-table schedule modification: pins one installment's interest/opening/closing balance to an
/// explicit value on a specific FacilityVersion (the same mechanism the engine already uses for
/// the 5 real borrowers whose historical schedules can't be reproduced from first principles).
/// Gated to CAD, audited like any other command.
/// </summary>
public sealed record ApplyInstallmentOverrideCommand(
    Guid FacilityId,
    Guid VersionId,
    int InstallmentIndex,
    decimal? InterestOverride,
    decimal? OpeningBalanceOverride,
    decimal? ClosingBalanceOverride) : IRequest, IAuditableAction, ITransactionalCommand;

public sealed class ApplyInstallmentOverrideCommandHandler : IRequestHandler<ApplyInstallmentOverrideCommand>
{
    private readonly IFacilityRepository _repository;
    private readonly ICurrentUserAccessor _currentUser;

    public ApplyInstallmentOverrideCommandHandler(IFacilityRepository repository, ICurrentUserAccessor currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public async Task Handle(ApplyInstallmentOverrideCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsInRole("CAD"))
            throw new UnauthorizedAccessException("Only a CAD user can modify a schedule.");

        var facility = await _repository.GetByIdAsync(request.FacilityId, cancellationToken)
            ?? throw new InvalidOperationException("Facility not found.");

        var version = facility.Versions.FirstOrDefault(v => v.Id == request.VersionId)
            ?? throw new InvalidOperationException("Facility version not found.");

        version.ApplyInstallmentOverride(
            request.InstallmentIndex, request.InterestOverride, request.OpeningBalanceOverride,
            request.ClosingBalanceOverride, _currentUser.UserId);
    }
}
