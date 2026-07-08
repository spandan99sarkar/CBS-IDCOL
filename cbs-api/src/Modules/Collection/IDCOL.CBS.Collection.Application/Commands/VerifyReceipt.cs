using IDCOL.CBS.BuildingBlocks.Application.Abstractions;
using IDCOL.CBS.Collection.Application.Abstractions;
using MediatR;

namespace IDCOL.CBS.Collection.Application.Commands;

/// <summary>Stage 2 (Accounts): reconcile and post the receipt to the GL, moving Pending -> Verified.</summary>
public sealed record VerifyReceiptCommand(Guid ReceiptId, string? Comment)
    : IRequest, IAuditableAction, ITransactionalCommand;

public sealed class VerifyReceiptCommandHandler : IRequestHandler<VerifyReceiptCommand>
{
    private readonly IReceiptRepository _repository;
    private readonly ICurrentUserAccessor _currentUser;

    public VerifyReceiptCommandHandler(IReceiptRepository repository, ICurrentUserAccessor currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public async Task Handle(VerifyReceiptCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsInRole("ACCOUNTS"))
            throw new UnauthorizedAccessException("Only an Accounts user can verify a collection.");

        var receipt = await _repository.GetByIdAsync(request.ReceiptId, cancellationToken)
            ?? throw new InvalidOperationException("Receipt not found.");

        // The system proposes the balanced GL posting: Dr Bank for the full receipt, and credits
        // to the loan principal / interest income / LPC income for each non-zero bucket.
        var lines = new List<(string, string, decimal, decimal)>
        {
            ("202030", "Bank Account", receipt.InstrumentAmount, 0m),
        };
        if (receipt.PrincipalAmount > 0) lines.Add(("102030", "Loan Principal Recovery", 0m, receipt.PrincipalAmount));
        if (receipt.InterestAmount > 0) lines.Add(("401010", "Interest Income", 0m, receipt.InterestAmount));
        if (receipt.LpcAmount > 0) lines.Add(("401020", "LPC Income", 0m, receipt.LpcAmount));

        var result = receipt.Verify(_currentUser.UserId, request.Comment, lines);
        if (result.IsFailure)
            throw new InvalidOperationException(result.Error);
    }
}
