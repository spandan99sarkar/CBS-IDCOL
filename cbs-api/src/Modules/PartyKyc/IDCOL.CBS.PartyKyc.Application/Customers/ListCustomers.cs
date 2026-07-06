using IDCOL.CBS.PartyKyc.Application.Abstractions;
using MediatR;

namespace IDCOL.CBS.PartyKyc.Application.Customers;

public sealed record CustomerDto(
    Guid Id, string CustomerNo, string CustomerType, string Name, string BusinessUnitCode,
    string? Mobile, string? Email, string? SectorCode, string KycStatus, string RiskLevel, string Source);

public sealed record ListCustomersQuery : IRequest<IReadOnlyList<CustomerDto>>;

public sealed class ListCustomersQueryHandler : IRequestHandler<ListCustomersQuery, IReadOnlyList<CustomerDto>>
{
    private readonly ICustomerRepository _repository;

    public ListCustomersQueryHandler(ICustomerRepository repository) => _repository = repository;

    public async Task<IReadOnlyList<CustomerDto>> Handle(ListCustomersQuery request, CancellationToken cancellationToken)
    {
        var customers = await _repository.ListAsync(cancellationToken);
        return customers
            .Select(c => new CustomerDto(
                c.Id, c.CustomerNo, c.CustomerType, c.Name, c.BusinessUnitCode,
                c.Mobile, c.Email, c.SectorCode, c.KycStatus, c.RiskLevel, c.Source))
            .ToList();
    }
}
