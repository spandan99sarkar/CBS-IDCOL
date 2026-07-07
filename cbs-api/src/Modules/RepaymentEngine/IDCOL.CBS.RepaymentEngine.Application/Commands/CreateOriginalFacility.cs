using System.Text.Json;
using FluentValidation;
using IDCOL.CBS.BuildingBlocks.Application.Abstractions;
using IDCOL.CBS.RepaymentEngine.Application.Abstractions;
using IDCOL.CBS.RepaymentEngine.Domain;
using MediatR;

namespace IDCOL.CBS.RepaymentEngine.Application.Commands;

/// <summary>
/// Creates a Facility and its ORIGINAL (version 0, as-sanctioned) schedule. Ordinarily called once
/// a sanction is signed and its schedule parameters are known; a co-financed project calls this
/// once per lender tranche (see Facility's multi-lender note).
/// </summary>
public sealed record CreateOriginalFacilityCommand(
    Guid SanctionId,
    string LenderCode,
    string Currency,
    DateOnly EffectiveDate,
    ScheduleParameters Parameters) : IRequest<Guid>, IAuditableAction, ITransactionalCommand;

public sealed class CreateOriginalFacilityCommandValidator : AbstractValidator<CreateOriginalFacilityCommand>
{
    public CreateOriginalFacilityCommandValidator()
    {
        RuleFor(x => x.SanctionId).NotEmpty();
        RuleFor(x => x.LenderCode).NotEmpty();
        RuleFor(x => x.Parameters.NumInstallments).GreaterThan(0);
        RuleFor(x => x.Parameters.Disbursements).NotEmpty();
    }
}

public sealed class CreateOriginalFacilityCommandHandler : IRequestHandler<CreateOriginalFacilityCommand, Guid>
{
    private readonly IFacilityRepository _repository;
    private readonly ICurrentUserAccessor _currentUser;

    public CreateOriginalFacilityCommandHandler(IFacilityRepository repository, ICurrentUserAccessor currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(CreateOriginalFacilityCommand request, CancellationToken cancellationToken)
    {
        var id = Guid.NewGuid();
        var facility = Facility.CreateOriginal(
            id, request.SanctionId, request.LenderCode, request.Currency, request.EffectiveDate,
            JsonSerializer.Serialize(request.Parameters), _currentUser.UserId);

        await _repository.AddAsync(facility, cancellationToken);
        return id;
    }
}
