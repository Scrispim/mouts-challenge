using Ambev.DeveloperEvaluation.Application.DTOs;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Commands.UpdateSale;

public class UpdateSaleCommandHandler(ISaleRepository saleRepository, IMediator mediator)
    : IRequestHandler<UpdateSaleCommand, SaleDto>
{
    public async Task<SaleDto> Handle(UpdateSaleCommand request, CancellationToken cancellationToken)
    {
        var sale = await saleRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Sale {request.Id} not found.");

        var items = request.Items.Select(i =>
            (i.ProductId, i.ProductName, i.Quantity, i.UnitPrice));

        sale.Update(
            request.SaleDate,
            request.CustomerId,
            request.CustomerName,
            request.BranchId,
            request.BranchName,
            items);

        await saleRepository.UpdateAsync(sale, cancellationToken);

        foreach (var domainEvent in sale.DomainEvents)
            await mediator.Publish(domainEvent, cancellationToken);

        sale.ClearDomainEvents();

        return sale.ToDto();
    }
}