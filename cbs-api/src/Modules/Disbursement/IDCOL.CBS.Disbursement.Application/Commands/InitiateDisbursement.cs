using FluentValidation;
using IDCOL.CBS.BuildingBlocks.Application.Abstractions;
using IDCOL.CBS.Disbursement.Application.Abstractions;
using IDCOL.CBS.Disbursement.Domain;
using MediatR;

namespace IDCOL.CBS.Disbursement.Application.Commands;

/// <summary>Stage 1 (BU): initiate a disbursement against a signed sanction.</summary>
public sealed record InitiateDisbursementCommand(
    Guid SanctionId,
    string SanctionRef,
    string CustomerNo,
    string ProjectName,
    string LoanCurrency,
    decimal SuggestedLoanAmount,
    decimal SuggestedGrantAmount,
    string? BuRemarks) : IRequest<Guid>, IAuditableAction, ITransactionalCommand;

public sealed class InitiateDisbursementCommandValidator : AbstractValidator<InitiateDisbursementCommand>
{
    public InitiateDisbursementCommandValidator()
    {
        RuleFor(x => x.SanctionId).NotEmpty();
        RuleFor(x => x.SanctionRef).NotEmpty();
        RuleFor(x => x.SuggestedLoanAmount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.SuggestedGrantAmount).GreaterThanOrEqualTo(0);
    }
}

public sealed class InitiateDisbursementCommandHandler : IRequestHandler<InitiateDisbursementCommand, Guid>
{
    private readonly IDisbursementRepository _repository;
    private readonly ICurrentUserAccessor _currentUser;

    public InitiateDisbursementCommandHandler(IDisbursementRepository repository, ICurrentUserAccessor currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(InitiateDisbursementCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsInRole("BU"))
            throw new UnauthorizedAccessException("Only a BU user can initiate a disbursement.");

        var count = await _repository.CountForSanctionAsync(request.SanctionId, cancellationToken);
        var disbursementNo = count + 1;
        var id = Guid.NewGuid();
        var referenceNo = $"DS-{DateTime.UtcNow:yyyyMMdd}-{id.ToString()[..8]}";

        var disbursement = DisbursementRequest.Initiate(
            id, referenceNo, disbursementNo, request.SanctionId, request.SanctionRef, request.CustomerNo,
            request.ProjectName, request.LoanCurrency, request.SuggestedLoanAmount, request.SuggestedGrantAmount,
            request.BuRemarks, _currentUser.UserId);

        await _repository.AddAsync(disbursement, cancellationToken);
        return id;
    }
}
