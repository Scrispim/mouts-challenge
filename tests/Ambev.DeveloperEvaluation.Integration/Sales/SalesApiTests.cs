using System.Net;
using System.Net.Http.Json;
using Ambev.DeveloperEvaluation.Application.Commands.CreateSale;
using Ambev.DeveloperEvaluation.Application.Commands.UpdateSale;
using Ambev.DeveloperEvaluation.Application.DTOs;
using Ambev.DeveloperEvaluation.WebApi.Common;
using FluentAssertions;

namespace Ambev.DeveloperEvaluation.Integration.Sales;

public class SalesApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public SalesApiTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    // ─── POST /api/sales ───────────────────────────────────────────────────────

    [Fact]
    public async Task CreateSale_ValidCommand_Returns201WithBody()
    {
        var command = BuildCreateCommand();

        var response = await _client.PostAsJsonAsync("/api/sales", command);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<ApiResponseWithData<SaleDto>>();
        body!.Success.Should().BeTrue();
        var dto = body.Data!;
        dto.SaleNumber.Should().Be(command.SaleNumber);
        dto.CustomerName.Should().Be(command.CustomerName);
        dto.Items.Should().HaveCount(1);
        dto.Items[0].Discount.Should().Be(0m);
        dto.Items[0].TotalAmount.Should().Be(2 * 100m);
    }

    [Fact]
    public async Task CreateSale_NoItems_Returns400()
    {
        var command = BuildCreateCommand();
        command.Items.Clear();

        var response = await _client.PostAsJsonAsync("/api/sales", command);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateSale_QuantityAbove20_Returns400()
    {
        var command = BuildCreateCommand(quantity: 21);

        var response = await _client.PostAsJsonAsync("/api/sales", command);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateSale_MissingSaleNumber_Returns400()
    {
        var command = BuildCreateCommand();
        command.SaleNumber = string.Empty;

        var response = await _client.PostAsJsonAsync("/api/sales", command);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateSale_AppliesDiscount_When4To9Items()
    {
        var command = BuildCreateCommand(quantity: 5);

        var response = await _client.PostAsJsonAsync("/api/sales", command);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<ApiResponseWithData<SaleDto>>();
        var dto = body!.Data!;
        dto.Items[0].Discount.Should().Be(0.10m);
        dto.Items[0].TotalAmount.Should().Be(5 * 100m * 0.90m);
    }

    // ─── GET /api/sales/{id} ──────────────────────────────────────────────────

    [Fact]
    public async Task GetSaleById_ExistingSale_Returns200WithBody()
    {
        var created = await CreateSaleAsync();

        var response = await _client.GetAsync($"/api/sales/{created.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ApiResponseWithData<SaleDto>>();
        body!.Success.Should().BeTrue();
        var dto = body.Data!;
        dto.Id.Should().Be(created.Id);
        dto.SaleNumber.Should().Be(created.SaleNumber);
    }

    [Fact]
    public async Task GetSaleById_NonExistingSale_Returns404()
    {
        var response = await _client.GetAsync($"/api/sales/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ─── GET /api/sales ───────────────────────────────────────────────────────

    [Fact]
    public async Task GetAllSales_Returns200WithPaginatedResult()
    {
        await CreateSaleAsync();
        await CreateSaleAsync();

        var response = await _client.GetAsync("/api/sales?_page=1&_size=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PaginatedResponse<SaleDto>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeEmpty();
        result.CurrentPage.Should().Be(1);
        result.TotalCount.Should().BeGreaterThan(0);
    }

    // ─── PUT /api/sales/{id} ──────────────────────────────────────────────────

    [Fact]
    public async Task UpdateSale_ValidCommand_Returns200WithUpdatedData()
    {
        var created = await CreateSaleAsync();
        var updateCommand = new UpdateSaleCommand
        {
            SaleDate = DateTime.UtcNow,
            CustomerId = Guid.NewGuid(),
            CustomerName = "Updated Customer",
            BranchId = Guid.NewGuid(),
            BranchName = "Updated Branch",
            Items =
            [
                new CreateSaleItemCommand
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "Updated Product",
                    Quantity = 10,
                    UnitPrice = 50m
                }
            ]
        };

        var response = await _client.PutAsJsonAsync($"/api/sales/{created.Id}", updateCommand);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ApiResponseWithData<SaleDto>>();
        var dto = body!.Data!;
        dto.CustomerName.Should().Be("Updated Customer");
        dto.Items[0].Discount.Should().Be(0.20m);
    }

    [Fact]
    public async Task UpdateSale_NonExistingSale_Returns404()
    {
        var updateCommand = new UpdateSaleCommand
        {
            SaleDate = DateTime.UtcNow,
            CustomerId = Guid.NewGuid(),
            CustomerName = "Customer",
            BranchId = Guid.NewGuid(),
            BranchName = "Branch",
            Items =
            [
                new CreateSaleItemCommand
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "Product",
                    Quantity = 1,
                    UnitPrice = 10m
                }
            ]
        };

        var response = await _client.PutAsJsonAsync($"/api/sales/{Guid.NewGuid()}", updateCommand);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ─── DELETE /api/sales/{id} ───────────────────────────────────────────────

    [Fact]
    public async Task CancelSale_ExistingSale_Returns200()
    {
        var created = await CreateSaleAsync();

        var response = await _client.DeleteAsync($"/api/sales/{created.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse>();
        body!.Success.Should().BeTrue();
    }

    [Fact]
    public async Task CancelSale_AlreadyCancelled_Returns400()
    {
        var created = await CreateSaleAsync();
        await _client.DeleteAsync($"/api/sales/{created.Id}");

        var response = await _client.DeleteAsync($"/api/sales/{created.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CancelSale_NonExistingSale_Returns404()
    {
        var response = await _client.DeleteAsync($"/api/sales/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────

    private async Task<SaleDto> CreateSaleAsync(int quantity = 2)
    {
        var response = await _client.PostAsJsonAsync("/api/sales", BuildCreateCommand(quantity));
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<ApiResponseWithData<SaleDto>>();
        return body!.Data!;
    }

    private static CreateSaleCommand BuildCreateCommand(int quantity = 2) => new()
    {
        SaleNumber = $"SALE-{Guid.NewGuid():N}",
        SaleDate = DateTime.UtcNow,
        CustomerId = Guid.NewGuid(),
        CustomerName = "Test Customer",
        BranchId = Guid.NewGuid(),
        BranchName = "Test Branch",
        Items =
        [
            new CreateSaleItemCommand
            {
                ProductId = Guid.NewGuid(),
                ProductName = "Test Product",
                Quantity = quantity,
                UnitPrice = 100m
            }
        ]
    };
}
