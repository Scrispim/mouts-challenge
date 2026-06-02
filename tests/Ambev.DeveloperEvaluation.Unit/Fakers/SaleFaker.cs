using Ambev.DeveloperEvaluation.Application.Commands.CreateSale;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Bogus;

namespace Ambev.DeveloperEvaluation.Unit.Fakers;

public static class SaleFaker
{
    private static readonly Faker F = new("pt_BR");

    public static Sale GenerateSale(int? quantity = null)
    {
        var qty = quantity ?? F.Random.Int(1, 3);
        var items = new[]
        {
            (F.Random.Guid(), F.Commerce.ProductName(), qty, Math.Round(F.Random.Decimal(10, 500), 2))
        };

        return Sale.Create(
            $"SALE-{F.Random.AlphaNumeric(8).ToUpper()}",
            F.Date.Recent(),
            F.Random.Guid(),
            F.Company.CompanyName(),
            F.Random.Guid(),
            F.Address.City(),
            items);
    }

    public static CreateSaleCommand GenerateCreateCommand(int? quantity = null) =>
        new Faker<CreateSaleCommand>("pt_BR")
            .RuleFor(x => x.SaleNumber, f => $"SALE-{f.Random.AlphaNumeric(8).ToUpper()}")
            .RuleFor(x => x.SaleDate, f => f.Date.Recent())
            .RuleFor(x => x.CustomerId, f => f.Random.Guid())
            .RuleFor(x => x.CustomerName, f => f.Company.CompanyName())
            .RuleFor(x => x.BranchId, f => f.Random.Guid())
            .RuleFor(x => x.BranchName, f => f.Address.City())
            .RuleFor(x => x.Items, _ => new List<CreateSaleItemCommand>
            {
                new Faker<CreateSaleItemCommand>("pt_BR")
                    .RuleFor(i => i.ProductId, f => f.Random.Guid())
                    .RuleFor(i => i.ProductName, f => f.Commerce.ProductName())
                    .RuleFor(i => i.Quantity, f => quantity ?? f.Random.Int(1, 3))
                    .RuleFor(i => i.UnitPrice, f => Math.Round(f.Random.Decimal(10, 500), 2))
                    .Generate()
            })
            .Generate();
}
