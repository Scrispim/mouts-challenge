using Ambev.DeveloperEvaluation.ORM;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Ambev.DeveloperEvaluation.Integration.Sales;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = Guid.NewGuid().ToString();

    public CustomWebApplicationFactory()
    {
        // Satisfies the ?? throw check in Program.cs before the host builds.
        // The actual connection is never used because ConfigureTestServices replaces DbContext with InMemory.
        Environment.SetEnvironmentVariable(
            "ConnectionStrings__DefaultConnection",
            "Host=localhost;Database=test_ignored;Username=user;Password=pass");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");

        builder.ConfigureTestServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<DefaultContext>));
            if (descriptor is not null)
                services.Remove(descriptor);

            services.AddDbContext<DefaultContext>(options =>
                options.UseInMemoryDatabase(_dbName));
        });
    }
}