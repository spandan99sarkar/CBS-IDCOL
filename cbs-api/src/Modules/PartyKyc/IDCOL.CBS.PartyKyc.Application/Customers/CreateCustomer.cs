using FluentValidation;
using IDCOL.CBS.BuildingBlocks.Application.Abstractions;
using IDCOL.CBS.PartyKyc.Application.Abstractions;
using IDCOL.CBS.PartyKyc.Domain;
using MediatR;

namespace IDCOL.CBS.PartyKyc.Application.Customers;

public sealed record CreateCustomerCommand(
    string CustomerNo,
    string CustomerType,
    string Name,
    string BusinessUnitCode,
    string? Mobile,
    string? Email,
    string? SectorCode,
    string KycStatus,
    string RiskLevel) : IRequest<Guid>, IAuditableAction, ITransactionalCommand;

public sealed class CreateCustomerCommandValidator : AbstractValidator<CreateCustomerCommand>
{
    public CreateCustomerCommandValidator()
    {
        RuleFor(x => x.CustomerNo).NotEmpty().MaximumLength(30);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.CustomerType)
            .Must(t => t is "Individual" or "Institutional" or "Joint")
            .WithMessage("Customer type must be Individual, Institutional, or Joint.");
        RuleFor(x => x.BusinessUnitCode).NotEmpty();
    }
}

public sealed class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, Guid>
{
    private readonly ICustomerRepository _repository;
    private readonly ICurrentUserAccessor _currentUser;

    public CreateCustomerCommandHandler(ICustomerRepository repository, ICurrentUserAccessor currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        var existing = await _repository.GetByCustomerNoAsync(request.CustomerNo, cancellationToken);
        if (existing is not null)
            throw new InvalidOperationException($"Customer number '{request.CustomerNo}' already exists.");

        var id = Guid.NewGuid();
        var customer = Customer.Create(
            id, request.CustomerNo, request.CustomerType, request.Name, request.BusinessUnitCode,
            request.Mobile, request.Email, request.SectorCode, request.KycStatus, request.RiskLevel,
            "Local", _currentUser.UserId);

        await _repository.AddAsync(customer, cancellationToken);
        return id;
    }
}
