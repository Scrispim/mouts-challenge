namespace Ambev.DeveloperEvaluation.Domain.Entities;

public class SaleItem
{
    private const int MaxQuantity = 20;
    private const int HighDiscountMinQuantity = 10;
    private const int LowDiscountMinQuantity = 4;
    private const decimal HighDiscountRate = 0.20m;
    private const decimal LowDiscountRate = 0.10m;

    public Guid Id { get; private set; }
    public Guid SaleId { get; private set; }

    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; } = string.Empty;

    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal Discount { get; private set; }
    public bool IsCancelled { get; private set; }

    public decimal TotalAmount => UnitPrice * Quantity * (1 - Discount);

    private SaleItem() { }

    public static SaleItem Create(Guid saleId, Guid productId, string productName, int quantity, decimal unitPrice)
    {
        if (quantity > MaxQuantity)
            throw new DomainException($"Cannot sell more than {MaxQuantity} identical items.");

        if (quantity <= 0)
            throw new DomainException("Quantity must be greater than zero.");

        if (unitPrice <= 0)
            throw new DomainException("Unit price must be greater than zero.");

        var discount = CalculateDiscount(quantity);

        return new SaleItem
        {
            Id = Guid.NewGuid(),
            SaleId = saleId,
            ProductId = productId,
            ProductName = productName,
            Quantity = quantity,
            UnitPrice = unitPrice,
            Discount = discount,
            IsCancelled = false
        };
    }

    public void Cancel()
    {
        if (IsCancelled)
            throw new InvalidOperationException("Item is already cancelled.");

        IsCancelled = true;
    }

    private static decimal CalculateDiscount(int quantity)
    {
        if (quantity >= HighDiscountMinQuantity)
            return HighDiscountRate;

        if (quantity >= LowDiscountMinQuantity)
            return LowDiscountRate;

        return 0m;
    }

}