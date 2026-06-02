using Ambev.DeveloperEvaluation.Domain.Repositories;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Commands.CanselSale;

public class CancelSaleItemCommandHandler(ISaleRepository saleRepository, IMediator mediator)
    : IRequestHandler<CancelSaleItemCommand>
{
    public async Task Handle(CancelSaleItemCommand request, CancellationToken cancellationToken)
    {
        var sale = await saleRepository.GetByIdAsync(request.SaleId, cancellationToken)
            ?? throw new KeyNotFoundException($"Sale {request.SaleId} not found.");

        sale.CancelItem(request.ItemId);

        await saleRepository.UpdateAsync(sale, cancellationToken);

        foreach (var domainEvent in sale.DomainEvents)
            await mediator.Publish(domainEvent, cancellationToken);

        sale.ClearDomainEvents();
    }
}