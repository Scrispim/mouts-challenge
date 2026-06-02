
using Ambev.DeveloperEvaluation.Application.DTOs;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Commands.CreateSale;

public class CreateSaleCommandHandler(ISaleRepository saleRepository, IMediator mediator) 
: IRequestHandler<CreateSaleCommand, SaleDto>
{
    public async Task<SaleDto> Handle(CreateSaleCommand request, CancellationToken cancellationToken)
    {
        // Create new sale entity
        var items = request.Items.Select(i =>
            (i.ProductId, i.ProductName, i.Quantity, i.UnitPrice));

        var sale = Sale.Create(
            request.SaleNumber,
            request.SaleDate,
            request.CustomerId,
            request.CustomerName,
            request.BranchId,
            request.BranchName,
            items);

        await saleRepository.AddAsync(sale, cancellationToken);

        foreach (var domainEvent in sale.DomainEvents)
            await mediator.Publish(domainEvent, cancellationToken);

        sale.ClearDomainEvents();

        return sale.ToDto();
    }
}