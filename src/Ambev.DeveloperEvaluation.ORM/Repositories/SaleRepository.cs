using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
namespace Ambev.DeveloperEvaluation.ORM.Repositories;

public class SaleRepository(DefaultContext context) : ISaleRepository
{
    public async Task<Sale?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await context.Sales
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);


    public async Task<Sale?> GetBySaleNumberAsync(string saleNumber, CancellationToken cancellationToken = default)
        => await context.Sales
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.SaleNumber == saleNumber, cancellationToken);

    public async Task<(IEnumerable<Sale> Items, int TotalCount)> GetAllAsync(
        int page, int pageSize, string? order = null, Dictionary<string, string>? filters = null,
        CancellationToken cancellationToken = default)
    {
        var query = context.Sales.Include(s => s.Items).AsNoTracking();

        query = ApplyFilters(query, filters);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await ApplyOrdering(query, order)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    private static IQueryable<Sale> ApplyFilters(IQueryable<Sale> query, Dictionary<string, string>? filters)
    {
        if (filters is null or { Count: 0 })
            return query;

        foreach (var (key, value) in filters)
        {
            switch (key.ToLower())
            {
                case "salenumber":
                    query = ApplyStringFilter(query, value,
                        v => s => s.SaleNumber.Contains(v),
                        v => s => s.SaleNumber.EndsWith(v),
                        v => s => s.SaleNumber.StartsWith(v),
                        v => s => s.SaleNumber == v);
                    break;
                case "customername":
                    query = ApplyStringFilter(query, value,
                        v => s => s.CustomerName.Contains(v),
                        v => s => s.CustomerName.EndsWith(v),
                        v => s => s.CustomerName.StartsWith(v),
                        v => s => s.CustomerName == v);
                    break;
                case "branchname":
                    query = ApplyStringFilter(query, value,
                        v => s => s.BranchName.Contains(v),
                        v => s => s.BranchName.EndsWith(v),
                        v => s => s.BranchName.StartsWith(v),
                        v => s => s.BranchName == v);
                    break;
                case "iscancelled" when bool.TryParse(value, out var cancelled):
                    query = query.Where(s => s.IsCancelled == cancelled);
                    break;
                case "_minsaledate" when DateTime.TryParse(value, out var minDate):
                    query = query.Where(s => s.SaleDate >= minDate);
                    break;
                case "_maxsaledate" when DateTime.TryParse(value, out var maxDate):
                    query = query.Where(s => s.SaleDate <= maxDate);
                    break;
            }
        }

        return query;
    }

    private static IQueryable<Sale> ApplyStringFilter(
        IQueryable<Sale> query,
        string value,
        Func<string, System.Linq.Expressions.Expression<Func<Sale, bool>>> contains,
        Func<string, System.Linq.Expressions.Expression<Func<Sale, bool>>> endsWith,
        Func<string, System.Linq.Expressions.Expression<Func<Sale, bool>>> startsWith,
        Func<string, System.Linq.Expressions.Expression<Func<Sale, bool>>> exact)
    {
        if (value.StartsWith('*') && value.EndsWith('*'))
            return query.Where(contains(value.Trim('*')));
        if (value.StartsWith('*'))
            return query.Where(endsWith(value.TrimStart('*')));
        if (value.EndsWith('*'))
            return query.Where(startsWith(value.TrimEnd('*')));
        return query.Where(exact(value));
    }

    private static IOrderedQueryable<Sale> ApplyOrdering(IQueryable<Sale> query, string? order)
    {
        if (string.IsNullOrWhiteSpace(order))
            return query.OrderByDescending(s => s.SaleDate);

        IOrderedQueryable<Sale>? result = null;

        foreach (var part in order.Trim('"').Split(',').Select(p => p.Trim()))
        {
            var segments = part.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var field = segments[0].ToLower();
            var desc = segments.Length > 1 && segments[1].ToLower() == "desc";

            if (result is null)
            {
                result = field switch
                {
                    "saledate"     => desc ? query.OrderByDescending(s => s.SaleDate)     : query.OrderBy(s => s.SaleDate),
                    "salenumber"   => desc ? query.OrderByDescending(s => s.SaleNumber)   : query.OrderBy(s => s.SaleNumber),
                    "customername" => desc ? query.OrderByDescending(s => s.CustomerName) : query.OrderBy(s => s.CustomerName),
                    "branchname"   => desc ? query.OrderByDescending(s => s.BranchName)   : query.OrderBy(s => s.BranchName),
                    _              => query.OrderByDescending(s => s.SaleDate)
                };
            }
            else
            {
                result = field switch
                {
                    "saledate"     => desc ? result.ThenByDescending(s => s.SaleDate)     : result.ThenBy(s => s.SaleDate),
                    "salenumber"   => desc ? result.ThenByDescending(s => s.SaleNumber)   : result.ThenBy(s => s.SaleNumber),
                    "customername" => desc ? result.ThenByDescending(s => s.CustomerName) : result.ThenBy(s => s.CustomerName),
                    "branchname"   => desc ? result.ThenByDescending(s => s.BranchName)   : result.ThenBy(s => s.BranchName),
                    _              => result
                };
            }
        }

        return result ?? query.OrderByDescending(s => s.SaleDate);
    }

    public async Task<Sale> AddAsync(Sale sale, CancellationToken cancellationToken = default)
    {
        await context.Sales.AddAsync(sale, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return sale;
    }

    public async Task<Sale> UpdateAsync(Sale sale, CancellationToken cancellationToken = default)
    {
        foreach (var item in sale.Items)
        {
            if (context.Entry(item).State == EntityState.Detached)
                context.SaleItems.Add(item);
        }

        await context.SaveChangesAsync(cancellationToken);
        return sale;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var sale = await GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Sale {id} not found.");

        context.Sales.Remove(sale);
        await context.SaveChangesAsync(cancellationToken);
    }

}
