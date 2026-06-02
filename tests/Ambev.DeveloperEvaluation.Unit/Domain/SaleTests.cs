using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Events;
using FluentAssertions;

namespace Ambev.DeveloperEvaluation.Unit.Domain;

public class SaleTests
{
    private static Sale CreateValidSale(IEnumerable<(Guid, string, int, decimal)>? items = null)
    {
        items ??= new[] { (Guid.NewGuid(), "Product A", 2, 50m) };
        return Sale.Create(
            "SALE-001",
            DateTime.UtcNow,
            Guid.NewGuid(),
            "Customer X",
            Guid.NewGuid(),
            "Branch Y",
            items);
    }

    [Fact]
    public void Create_ShouldRaiseSaleCreatedEvent()
    {
        var sale = CreateValidSale();

        sale.DomainEvents.Should().ContainSingle(e => e is SaleCreatedEvent);
    }

    [Fact]
    public void Create_ShouldCalculateTotalAmountCorrectly()
    {
        var items = new[]
        {
            (Guid.NewGuid(), "Product A", 2, 100m),
            (Guid.NewGuid(), "Product B", 5, 50m)
        };
        var sale = Sale.Create("SALE-001", DateTime.UtcNow, Guid.NewGuid(), "Customer", Guid.NewGuid(), "Branch", items);

        sale.TotalAmount.Should().Be(2 * 100m + 5 * 50m * 0.90m);
    }

    [Fact]
    public void Cancel_ShouldSetIsCancelledAndRaiseSaleCancelledEvent()
    {
        var sale = CreateValidSale();
        sale.ClearDomainEvents();

        sale.Cancel();

        sale.IsCancelled.Should().BeTrue();
        sale.DomainEvents.Should().ContainSingle(e => e is SaleCancelledEvent);
    }

    [Fact]
    public void Cancel_AlreadyCancelled_ShouldThrow()
    {
        var sale = CreateValidSale();
        sale.Cancel();

        var act = () => sale.Cancel();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void CancelItem_ShouldCancelSpecificItemAndRaiseItemCancelledEvent()
    {
        var items = new[]
        {
            (Guid.NewGuid(), "Product A", 2, 100m),
            (Guid.NewGuid(), "Product B", 3, 50m)
        };
        var sale = Sale.Create("SALE-001", DateTime.UtcNow, Guid.NewGuid(), "Customer", Guid.NewGuid(), "Branch", items);
        sale.ClearDomainEvents();

        var itemId = sale.Items.First().Id;
        sale.CancelItem(itemId);

        sale.Items.First(i => i.Id == itemId).IsCancelled.Should().BeTrue();
        sale.DomainEvents.Should().ContainSingle(e => e is ItemCancelledEvent);
    }

    [Fact]
    public void Update_CancelledSale_ShouldThrow()
    {
        var sale = CreateValidSale();
        sale.Cancel();

        var act = () => sale.Update(
            DateTime.UtcNow, Guid.NewGuid(), "New Customer", Guid.NewGuid(), "New Branch",
            new[] { (Guid.NewGuid(), "Product", 1, 10m) });

        act.Should().Throw<InvalidOperationException>();
    }
}