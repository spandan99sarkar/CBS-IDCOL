using IDCOL.CBS.ProductConfig.Application.Abstractions;
using MediatR;

namespace IDCOL.CBS.ProductConfig.Application.Products;

public sealed record ProductDto(
    Guid Id, string ProductCode, string ProductName, string ProductType, string InterestType,
    string RepaymentMethod, int DayCountBasis, decimal SuggestedRatePercent, bool IsActive);

public sealed record ListProductsQuery : IRequest<IReadOnlyList<ProductDto>>;

public sealed class ListProductsQueryHandler : IRequestHandler<ListProductsQuery, IReadOnlyList<ProductDto>>
{
    private readonly ILoanProductRepository _repository;

    public ListProductsQueryHandler(ILoanProductRepository repository) => _repository = repository;

    public async Task<IReadOnlyList<ProductDto>> Handle(ListProductsQuery request, CancellationToken cancellationToken)
    {
        var products = await _repository.ListAsync(cancellationToken);
        return products
            .Select(p => new ProductDto(
                p.Id, p.ProductCode, p.ProductName, p.ProductType, p.InterestType,
                p.RepaymentMethod, p.DayCountBasis, p.SuggestedRatePercent, p.IsActive))
            .ToList();
    }
}
