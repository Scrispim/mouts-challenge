using Ambev.DeveloperEvaluation.Application.DTOs;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.queries.GetSale;

public class GetSalesQueryHandler(ISaleRepository saleRepository)
    : IRequestHandler<GetSalesQuery, GetSalesResult>
{
    public async Task<GetSalesResult> Handle(GetSalesQuery request, CancellationToken cancellationToken)
    {
        var (sales, totalCount) = await saleRepository.GetAllAsync(request.Page, request.PageSize, request.Order, request.Filters, cancellationToken);

        return new GetSalesResult
        {
            Items = sales.Select(sale => sale.ToDto()),
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}
