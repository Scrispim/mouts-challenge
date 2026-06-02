using AutoMapper;
using Ambev.DeveloperEvaluation.Application.Commands.CreateSale;
using Ambev.DeveloperEvaluation.Application.Mappings;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Unit.Fakers;
using FluentAssertions;
using MediatR;
using NSubstitute;

namespace Ambev.DeveloperEvaluation.Unit.Application;

public class CreateSaleCommandHandlerTests
{
    private readonly ISaleRepository _repository = Substitute.For<ISaleRepository>();
    private readonly IMediator _mediator = Substitute.For<IMediator>();
    private readonly IMapper _mapper = new MapperConfiguration(cfg => cfg.AddProfile<SaleProfile>()).CreateMapper();
    private readonly CreateSaleCommandHandler _handler;

    public CreateSaleCommandHandlerTests()
    {
        _handler = new CreateSaleCommandHandler(_repository, _mediator, _mapper);
        _repository.AddAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>())
            .Returns(x => x.Arg<Sale>());
    }

    [Fact]
    public async Task Handle_ValidCommand_PersistsSaleWithCorrectNumber()
    {
        var command = SaleFaker.GenerateCreateCommand();

        await _handler.Handle(command, CancellationToken.None);

        await _repository.Received(1).AddAsync(
            Arg.Is<Sale>(s => s.SaleNumber == command.SaleNumber),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ValidCommand_PublishesSaleCreatedEvent()
    {
        var command = SaleFaker.GenerateCreateCommand();

        await _handler.Handle(command, CancellationToken.None);

        await _mediator.Received(1).Publish(
            Arg.Any<SaleCreatedEvent>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsDtoMappedFromSale()
    {
        var command = SaleFaker.GenerateCreateCommand();

        var result = await _handler.Handle(command, CancellationToken.None);

        result.SaleNumber.Should().Be(command.SaleNumber);
        result.CustomerName.Should().Be(command.CustomerName);
        result.BranchName.Should().Be(command.BranchName);
        result.Items.Should().HaveCount(command.Items.Count);
        result.IsCancelled.Should().BeFalse();
    }

    [Theory]
    [InlineData(1, 0.00)]
    [InlineData(5, 0.10)]
    [InlineData(12, 0.20)]
    public async Task Handle_ValidCommand_AppliesCorrectDiscount(int quantity, double expectedDiscount)
    {
        var command = SaleFaker.GenerateCreateCommand(quantity);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Items[0].Discount.Should().Be((decimal)expectedDiscount);
    }

}