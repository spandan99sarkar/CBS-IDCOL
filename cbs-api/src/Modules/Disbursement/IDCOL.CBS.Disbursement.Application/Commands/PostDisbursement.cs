using IDCOL.CBS.BuildingBlocks.Application.Abstractions;
using IDCOL.CBS.Disbursement.Application.Abstractions;
using MediatR;

namespace IDCOL.CBS.Disbursement.Application.Commands;

public sealed record GlLineInput(string GlCode, string Description, decimal Debit, decimal Credit);

/// <summary>Stage 3 (Accounts): post the disbursement to the GL, moving Proposed -> Processed.</summary>
public sealed record PostDisbursementCommand(
    Guid DisbursementId,
    string DisbursementMode,
    DateOnly ValueDate,
    IReadOnlyList<GlLineInput> GlLines) : IRequest, IAuditableAction, ITransactionalCommand;

public sealed class PostDisbursementCommandHandler : IRequestHandler<PostDisbursementCommand>
{
    private readonly IDisbursementRepository _repository;
    private readonly ICurrentUserAccessor _currentUser;

    public PostDisbursementCommandHandler(IDisbursementRepository repository, ICurrentUserAccessor currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public async Task Handle(PostDisbursementCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsInRole("ACCOUNTS"))
            throw new UnauthorizedAccessException("Only an Accounts user can post a disbursement.");

        var disbursement = await _repository.GetByIdAsync(request.DisbursementId, cancellationToken)
            ?? throw new InvalidOperationException("Disbursement not found.");

        var lines = request.GlLines
            .Select(l => (l.GlCode, l.Description, l.Debit, l.Credit))
            .ToList();

        var result = disbursement.Post(_currentUser.UserId, request.DisbursementMode, request.ValueDate, lines);
        if (result.IsFailure)
            throw new InvalidOperationException(result.Error);
    }
}
