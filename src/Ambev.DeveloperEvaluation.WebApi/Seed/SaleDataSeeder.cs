using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.ORM;
using Bogus;
using Microsoft.EntityFrameworkCore;

namespace Ambev.DeveloperEvaluation.WebApi.Seed;

public static class SaleDataSeeder
{
    private const int SeedCount = 50;

    public static async Task SeedAsync(DefaultContext context)
    {
        if (await context.Sales.AnyAsync())
            return;

        var faker = new Faker("pt_BR");
        var sales = new List<Sale>(SeedCount);

        for (var i = 0; i < SeedCount; i++)
        {
            var itemCount = faker.Random.Int(1, 4);
            var items = Enumerable.Range(0, itemCount).Select(_ =>
                (faker.Random.Guid(),
                 faker.Commerce.ProductName(),
                 faker.Random.Int(1, 15),
                 Math.Round(faker.Random.Decimal(10, 500), 2)));

            var sale = Sale.Create(
                $"SALE-{faker.Random.AlphaNumeric(8).ToUpper()}",
                faker.Date.Recent(90).ToUniversalTime(),
                faker.Random.Guid(),
                faker.Company.CompanyName(),
                faker.Random.Guid(),
                faker.Address.City(),
                items);

            if (faker.Random.Bool(0.2f))
                sale.Cancel();

            sale.ClearDomainEvents();
            sales.Add(sale);
        }

        await context.Sales.AddRangeAsync(sales);
        await context.SaveChangesAsync();
    }
}