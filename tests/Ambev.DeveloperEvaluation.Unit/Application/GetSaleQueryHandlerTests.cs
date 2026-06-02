using AutoMapper;
using Ambev.DeveloperEvaluation.Application.Mappings;
using Ambev.DeveloperEvaluation.Application.queries.GetSale;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Unit.Fakers;
using FluentAssertions;
using NSubstitute;

namespace Ambev.DeveloperEvaluation.Unit.Application;

public class GetSaleQueryHandlerTests
{
    private readonly ISaleRepository _repository = Substitute.For<ISaleRepository>();
    private readonly IMapper _mapper = new MapperConfiguration(cfg => cfg.AddProfile<SaleProfile>()).CreateMapper();
    private readonly GetSaleQueryHandler _handler;

    public GetSaleQueryHandlerTests()
    {
        _handler = new GetSaleQueryHandler(_repository, _mapper);
    }

    [Fact]
    public async Task Handle_ExistingSale_ReturnsMappedDto()
    {
        var sale = SaleFaker.GenerateSale();
        _repository.GetByIdAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);

        var result = await _handler.Handle(new GetSaleQuery(sale.Id), CancellationToken.None);

        result.Should().NotBeNull();
        result!.Id.Should().Be(sale.Id);
        result.SaleNumber.Should().Be(sale.SaleNumber);
        result.CustomerName.Should().Be(sale.CustomerName);
        result.Items.Should().HaveCount(sale.Items.Count);
    }

    [Fact]
    public async Task Handle_SaleNotFound_ReturnsNull()
    {
        _repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((Sale?)null);

        var result = await _handler.Handle(new GetSaleQuery(Guid.NewGuid()), CancellationToken.None);

        result.Should().BeNull();
    }
}
