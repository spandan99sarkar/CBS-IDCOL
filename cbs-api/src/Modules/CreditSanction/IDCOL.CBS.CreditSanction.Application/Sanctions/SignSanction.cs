using IDCOL.CBS.BuildingBlocks.Application.Abstractions;
using IDCOL.CBS.CreditSanction.Application.Abstractions;
using MediatR;

namespace IDCOL.CBS.CreditSanction.Application.Sanctions;

public sealed record SignSanctionCommand(Guid SanctionId) : IRequest, IAuditableAction, ITransactionalCommand;

public sealed class SignSanctionCommandHandler : IRequestHandler<SignSanctionCommand>
{
    private readonly ILoanAgreementRepository _repository;
    private readonly ICurrentUserAccessor _currentUser;

    public SignSanctionCommandHandler(ILoanAgreementRepository repository, ICurrentUserAccessor currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public async Task Handle(SignSanctionCommand request, CancellationToken cancellationToken)
    {
        var agreement = await _repository.GetByIdAsync(request.SanctionId, cancellationToken)
            ?? throw new InvalidOperationException("Sanction not found.");

        var result = agreement.Sign(_currentUser.UserId);
        if (result.IsFailure)
            throw new InvalidOperationException(result.Error);
    }
}
