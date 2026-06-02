using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Commands.CanselSale;

public record CancelSaleItemCommand(Guid SaleId, Guid ItemId) : IRequest;