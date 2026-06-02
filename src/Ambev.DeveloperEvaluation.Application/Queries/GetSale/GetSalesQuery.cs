using Ambev.DeveloperEvaluation.Application.DTOs;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.queries.GetSale;

public record GetSalesQuery(
    int Page = 1,
    int PageSize = 10,
    string? Order = null,
    Dictionary<string, string>? Filters = null) : IRequest<GetSalesResult>;

public class GetSalesResult
{
    public IEnumerable<SaleDto> Items { get; set; } = Enumerable.Empty<SaleDto>();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}