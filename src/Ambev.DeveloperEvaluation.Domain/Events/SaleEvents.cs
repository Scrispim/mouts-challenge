using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.Domain.Events;

public record SaleCreatedEvent(Guid SaleId, string SaleNumber) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record SaleModifiedEvent(Guid SaleId, string SaleNumber) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record SaleCancelledEvent(Guid SaleId, string SaleNumber) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record ItemCancelledEvent(Guid SaleId, string SaleNumber, Guid ItemId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}