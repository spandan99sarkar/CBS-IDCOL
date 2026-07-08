using FluentValidation;
using IDCOL.CBS.BuildingBlocks.Application.Abstractions;
using IDCOL.CBS.ProductConfig.Application.Abstractions;
using IDCOL.CBS.ProductConfig.Domain;
using MediatR;

namespace IDCOL.CBS.ProductConfig.Application.Products;

public sealed record CreateProductCommand(
    string ProductCode,
    string ProductName,
    string ProductType,
    string InterestType,
    string RepaymentMethod,
    int DayCountBasis,
    int GracePeriodMonths,
    bool PrepaymentAllowed,
    bool PenaltyAllowed,
    decimal SuggestedRatePercent,
    decimal LowerRatePercent,
    decimal UpperRatePercent) : IRequest<Guid>, IAuditableAction, ITransactionalCommand;

public sealed class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.ProductCode).NotEmpty().MaximumLength(30);
        RuleFor(x => x.ProductName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ProductType).NotEmpty();
        RuleFor(x => x.InterestType).Must(t => t is "Fixed" or "Floating").WithMessage("Interest type must be Fixed or Floating.");
        RuleFor(x => x.DayCountBasis).Must(b => b is 360 or 364 or 365);
        RuleFor(x => x.LowerRatePercent).LessThanOrEqualTo(x => x.UpperRatePercent);
    }
}

public sealed class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Guid>
{
    private readonly ILoanProductRepository _repository;
    private readonly ICurrentUserAccessor _currentUser;

    public CreateProductCommandHandler(ILoanProductRepository repository, ICurrentUserAccessor currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var existing = await _repository.GetByCodeAsync(request.ProductCode, cancellationToken);
        if (existing is not null)
            throw new InvalidOperationException($"Product code '{request.ProductCode}' already exists.");

        var id = Guid.NewGuid();
        var product = LoanProduct.Create(
            id, request.ProductCode, request.ProductName, request.ProductType, request.InterestType,
            request.RepaymentMethod, request.DayCountBasis, request.GracePeriodMonths, request.PrepaymentAllowed,
            request.PenaltyAllowed, request.SuggestedRatePercent, request.LowerRatePercent, request.UpperRatePercent,
            _currentUser.UserId);

        await _repository.AddAsync(product, cancellationToken);
        return id;
    }
}
