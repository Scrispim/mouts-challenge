

using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.Application.DTOs;

public static class SaleMappingExtensions
{
    public static SaleDto ToDto(this Sale sale) => new()
    {
        Id = sale.Id,
        SaleNumber = sale.SaleNumber,
        SaleDate = sale.SaleDate,
        CustomerId = sale.CustomerId,
        CustomerName = sale.CustomerName,
        BranchId = sale.BranchId,
        BranchName = sale.BranchName,
        TotalAmount = sale.TotalAmount,
        IsCancelled = sale.IsCancelled,
        Items = sale.Items.Select(i => i.ToDto()).ToList()
    };

    public static SaleItemDto ToDto(this SaleItem item) => new()
    {
        Id = item.Id,
        ProductId = item.ProductId,
        ProductName = item.ProductName,
        Quantity = item.Quantity,
        UnitPrice = item.UnitPrice,
        Discount = item.Discount,
        TotalAmount = item.TotalAmount,
        IsCancelled = item.IsCancelled
    };
}
