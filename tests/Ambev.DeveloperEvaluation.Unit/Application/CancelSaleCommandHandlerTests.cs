using Ambev.DeveloperEvaluation.Application.Commands.CanselSale;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Unit.Fakers;
using FluentAssertions;
using MediatR;
using NSubstitute;

namespace Ambev.DeveloperEvaluation.Unit.Application;

public class CancelSaleCommandHandlerTests
{
    private readonly ISaleRepository _repository = Substitute.For<ISaleRepository>();
    private readonly IMediator _mediator = Substitute.For<IMediator>();
    private readonly CancelSaleCommandHandler _handler;

    public CancelSaleCommandHandlerTests()
    {
        _handler = new CancelSaleCommandHandler(_repository, _mediator);
    }

    [Fact]
    public async Task Handle_ExistingSale_MarksSaleAsCancelled()
    {
        var sale = SaleFaker.GenerateSale();
        _repository.GetByIdAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);
        _repository.UpdateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>()).Returns(sale);

        await _handler.Handle(new CancelSaleCommand(sale.Id), CancellationToken.None);

        sale.IsCancelled.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ExistingSale_PersistsUpdate()
    {
        var sale = SaleFaker.GenerateSale();
        _repository.GetByIdAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);
        _repository.UpdateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>()).Returns(sale);

        await _handler.Handle(new CancelSaleCommand(sale.Id), CancellationToken.None);

        await _repository.Received(1).UpdateAsync(
            Arg.Is<Sale>(s => s.IsCancelled),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ExistingSale_PublishesSaleCancelledEvent()
    {
        var sale = SaleFaker.GenerateSale();
        _repository.GetByIdAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);
        _repository.UpdateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>()).Returns(sale);

        await _handler.Handle(new CancelSaleCommand(sale.Id), CancellationToken.None);

        await _mediator.Received(1).Publish(
            Arg.Any<SaleCancelledEvent>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_SaleNotFound_ThrowsKeyNotFoundException()
    {
        _repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((Sale?)null);

        var act = () => _handler.Handle(
            new CancelSaleCommand(Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
