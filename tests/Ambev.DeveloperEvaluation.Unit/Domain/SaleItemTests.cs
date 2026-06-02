using Ambev.DeveloperEvaluation.Domain.Entities;
using FluentAssertions;

namespace Ambev.DeveloperEvaluation.Unit.Domain;

public class SaleItemTests
{
    [Fact]
    public void Create_WithQuantityBelow4_ShouldHaveNoDiscount()
    {
        var item = SaleItem.Create(Guid.NewGuid(), Guid.NewGuid(), "Product A", 3, 100m);

        item.Discount.Should().Be(0m);
        item.TotalAmount.Should().Be(300m);
    }

    [Theory]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(9)]
    public void Create_WithQuantityBetween4And9_ShouldHave10PercentDiscount(int quantity)
    {
        var item = SaleItem.Create(Guid.NewGuid(), Guid.NewGuid(), "Product A", quantity, 100m);

        item.Discount.Should().Be(0.10m);
        item.TotalAmount.Should().Be(quantity * 100m * 0.90m);
    }

    [Theory]
    [InlineData(10)]
    [InlineData(15)]
    [InlineData(20)]
    public void Create_WithQuantityBetween10And20_ShouldHave20PercentDiscount(int quantity)
    {
        var item = SaleItem.Create(Guid.NewGuid(), Guid.NewGuid(), "Product A", quantity, 100m);

        item.Discount.Should().Be(0.20m);
        item.TotalAmount.Should().Be(quantity * 100m * 0.80m);
    }

    [Fact]
    public void Create_WithQuantityAbove20_ShouldThrowDomainException()
    {
        var act = () => SaleItem.Create(Guid.NewGuid(), Guid.NewGuid(), "Product A", 21, 100m);

        act.Should().Throw<DomainException>()
            .WithMessage("*20*");
    }

    [Fact]
    public void Cancel_ShouldSetIsCancelledToTrue()
    {
        var item = SaleItem.Create(Guid.NewGuid(), Guid.NewGuid(), "Product A", 5, 100m);
        item.Cancel();

        item.IsCancelled.Should().BeTrue();
    }

    [Fact]
    public void Cancel_AlreadyCancelled_ShouldThrowInvalidOperationException()
    {
        var item = SaleItem.Create(Guid.NewGuid(), Guid.NewGuid(), "Product A", 5, 100m);
        item.Cancel();

        var act = () => item.Cancel();

        act.Should().Throw<InvalidOperationException>();
    }
}
