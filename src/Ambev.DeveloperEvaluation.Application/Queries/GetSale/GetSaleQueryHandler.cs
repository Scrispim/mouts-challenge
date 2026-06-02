using Ambev.DeveloperEvaluation.Application.DTOs;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.queries.GetSale;

public class GetSaleQueryHandler(ISaleRepository saleRepository)
    : IRequestHandler<GetSaleQuery, SaleDto?>
{
    public async Task<SaleDto?> Handle(GetSaleQuery request, CancellationToken cancellationToken)
    {
        var sale = await saleRepository.GetByIdAsync(request.Id, cancellationToken);
        if (sale is null) return null;

        return sale.ToDto();
    }
}