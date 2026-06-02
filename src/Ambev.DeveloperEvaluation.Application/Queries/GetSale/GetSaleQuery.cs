using Ambev.DeveloperEvaluation.Application.DTOs;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.queries.GetSale;

public record GetSaleQuery(Guid Id) : IRequest<SaleDto?>;