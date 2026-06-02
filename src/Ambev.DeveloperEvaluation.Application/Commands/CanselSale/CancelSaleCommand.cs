using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Commands.CanselSale;


public record CancelSaleCommand(Guid Id) : IRequest;