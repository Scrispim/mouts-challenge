using Ambev.DeveloperEvaluation.Domain.Events;

namespace Ambev.DeveloperEvaluation.Domain.Entities;

public class Sale
{
    public Guid Id { get; private set; }
    public string SaleNumber { get; private set; } = string.Empty;
    public DateTime SaleDate { get; private set; }

    public Guid CustomerId { get; private set; }
    public string CustomerName { get; private set; } = string.Empty;

    public Guid BranchId { get; private set; }
    public string BranchName { get; private set; } = string.Empty;

    public bool IsCancelled { get; private set; }

    private readonly List<SaleItem> _items = new();
    public IReadOnlyCollection<SaleItem> Items => _items.AsReadOnly();

    public decimal TotalAmount => _items.Where(i => !i.IsCancelled).Sum(i => i.TotalAmount);

    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    private Sale() { }

    public static Sale Create(
        string saleNumber,
        DateTime saleDate,
        Guid customerId,
        string customerName,
        Guid branchId,
        string branchName,
        IEnumerable<(Guid ProductId, string ProductName, int Quantity, decimal UnitPrice)> items)
    {
        var sale = new Sale
        {
            Id = Guid.NewGuid(),
            SaleNumber = saleNumber,
            SaleDate = saleDate,
            CustomerId = customerId,
            CustomerName = customerName,
            BranchId = branchId,
            BranchName = branchName,
            IsCancelled = false
        };

        foreach (var item in items)
            sale.AddItem(item.ProductId, item.ProductName, item.Quantity, item.UnitPrice);

        sale._domainEvents.Add(new SaleCreatedEvent(sale.Id, sale.SaleNumber));

        return sale;
    }

    public void Update(
        DateTime saleDate,
        Guid customerId,
        string customerName,
        Guid branchId,
        string branchName,
        IEnumerable<(Guid ProductId, string ProductName, int Quantity, decimal UnitPrice)> items)
    {
        if (IsCancelled)
            throw new InvalidOperationException("Cannot update a cancelled sale.");

        SaleDate = saleDate;
        CustomerId = customerId;
        CustomerName = customerName;
        BranchId = branchId;
        BranchName = branchName;

        _items.Clear();
        foreach (var item in items)
            AddItem(item.ProductId, item.ProductName, item.Quantity, item.UnitPrice);

        _domainEvents.Add(new SaleModifiedEvent(Id, SaleNumber));
    }

    public void Cancel()
    {
        if (IsCancelled)
            throw new InvalidOperationException("Sale is already cancelled.");

        IsCancelled = true;
        foreach (var item in _items.Where(i => !i.IsCancelled))
            item.Cancel();

        _domainEvents.Add(new SaleCancelledEvent(Id, SaleNumber));
    }

    public void CancelItem(Guid itemId)
    {
        if (IsCancelled)
            throw new InvalidOperationException("Cannot cancel item of a cancelled sale.");

        var item = _items.FirstOrDefault(i => i.Id == itemId)
            ?? throw new KeyNotFoundException($"Item {itemId} not found in sale.");

        item.Cancel();
        _domainEvents.Add(new ItemCancelledEvent(Id, SaleNumber, itemId));
    }

    private void AddItem(Guid productId, string productName, int quantity, decimal unitPrice)
    {
        var item = SaleItem.Create(Id, productId, productName, quantity, unitPrice);
        _items.Add(item);
    }

    public void ClearDomainEvents() => _domainEvents.Clear();
    
}