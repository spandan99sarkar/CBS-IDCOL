using IDCOL.CBS.BuildingBlocks.Application.Abstractions;
using IDCOL.CBS.Classification.Application.Abstractions;
using IDCOL.CBS.Classification.Domain;
using MediatR;

namespace IDCOL.CBS.Classification.Application.Commands;

public sealed record ClassificationAccountInput(
    Guid AccountId,
    string AccountRef,
    string CustomerNo,
    string ProjectName,
    string Currency,
    string FinanceType,
    int TenorMonths,
    bool IsCmsme,
    decimal Outstanding,
    decimal OverdueMonths,
    decimal InterestSuspense,
    decimal EligibleCollateral,
    string? QualitativeOverride);

/// <summary>
/// Runs the DFIM 04/2021 classification &amp; provisioning engine over a set of loan accounts
/// as of a date, persisting one classification row per account. Returns the run id.
/// </summary>
public sealed record RunClassificationCommand(DateOnly AsOfDate, IReadOnlyList<ClassificationAccountInput> Accounts)
    : IRequest<Guid>, IAuditableAction, ITransactionalCommand;

public sealed class RunClassificationCommandHandler : IRequestHandler<RunClassificationCommand, Guid>
{
    private readonly IClassificationRepository _repository;
    private readonly ICurrentUserAccessor _currentUser;

    public RunClassificationCommandHandler(IClassificationRepository repository, ICurrentUserAccessor currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(RunClassificationCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsInRole("CAD"))
            throw new UnauthorizedAccessException("Only a CAD user can run classification.");

        var thresholds = await _repository.GetThresholdsAsync(cancellationToken);
        var rates = await _repository.GetRatesAsync(cancellationToken);
        var runId = Guid.NewGuid();

        foreach (var a in request.Accounts)
        {
            var status = ClassificationEngine.Classify(
                a.FinanceType, a.TenorMonths, a.OverdueMonths, thresholds, a.QualitativeOverride);

            var provision = ClassificationEngine.ComputeProvision(
                status, a.IsCmsme, a.Outstanding, a.InterestSuspense, a.EligibleCollateral, rates);

            var classification = LoanClassification.Create(
                Guid.NewGuid(), runId, request.AsOfDate, a.AccountId, a.AccountRef, a.CustomerNo, a.ProjectName,
                a.Currency, a.FinanceType, a.TenorMonths, TenorBucket.For(a.FinanceType, a.TenorMonths), a.IsCmsme,
                a.Outstanding, a.OverdueMonths, a.InterestSuspense, a.EligibleCollateral, status,
                !string.IsNullOrWhiteSpace(a.QualitativeOverride), a.QualitativeOverride, provision, _currentUser.UserId);

            await _repository.AddAsync(classification, cancellationToken);
        }

        return runId;
    }
}
