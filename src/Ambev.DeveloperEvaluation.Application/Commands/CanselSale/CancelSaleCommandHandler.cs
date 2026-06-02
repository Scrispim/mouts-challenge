using Ambev.DeveloperEvaluation.Domain.Repositories;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Commands.CanselSale;

public class CancelSaleCommandHandler(ISaleRepository saleRepository, IMediator mediator)
    : IRequestHandler<CancelSaleCommand>
{
    public async Task Handle(CancelSaleCommand request, CancellationToken cancellationToken)
    {
        var sale = await saleRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Sale {request.Id} not found.");

        sale.Cancel();

        await saleRepository.UpdateAsync(sale, cancellationToken);

        foreach (var domainEvent in sale.DomainEvents)
            await mediator.Publish(domainEvent, cancellationToken);

        sale.ClearDomainEvents();
    }
}