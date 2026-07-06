using IDCOL.CBS.BuildingBlocks.Application.Abstractions;
using IDCOL.CBS.Disbursement.Application.Abstractions;
using MediatR;

namespace IDCOL.CBS.Disbursement.Application.Commands;

/// <summary>Stage 2 (CAD): review and justify the amounts, moving Suggested -> Proposed.</summary>
public sealed record ReviewDisbursementCommand(
    Guid DisbursementId,
    decimal JustifiedLoanAmount,
    decimal JustifiedGrantAmount,
    string? CadRemarks) : IRequest, IAuditableAction, ITransactionalCommand;

public sealed class ReviewDisbursementCommandHandler : IRequestHandler<ReviewDisbursementCommand>
{
    private readonly IDisbursementRepository _repository;
    private readonly ICurrentUserAccessor _currentUser;

    public ReviewDisbursementCommandHandler(IDisbursementRepository repository, ICurrentUserAccessor currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public async Task Handle(ReviewDisbursementCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsInRole("CAD"))
            throw new UnauthorizedAccessException("Only a CAD user can review a disbursement.");

        var disbursement = await _repository.GetByIdAsync(request.DisbursementId, cancellationToken)
            ?? throw new InvalidOperationException("Disbursement not found.");

        var result = disbursement.Propose(
            _currentUser.UserId, request.JustifiedLoanAmount, request.JustifiedGrantAmount, request.CadRemarks);
        if (result.IsFailure)
            throw new InvalidOperationException(result.Error);
    }
}
