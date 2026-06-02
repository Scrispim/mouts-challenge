using Ambev.DeveloperEvaluation.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Ambev.DeveloperEvaluation.Application.EventHandlers;

public class SaleCreatedEventHandler(ILogger<SaleCreatedEventHandler> logger)
    : INotificationHandler<SaleCreatedEvent>
{
    public Task Handle(SaleCreatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "[SaleCreated] Sale {SaleNumber} (Id: {SaleId}) was created at {OccurredOn}",
            notification.SaleNumber, notification.SaleId, notification.OccurredOn);

        return Task.CompletedTask;
    }
}

public class SaleModifiedEventHandler(ILogger<SaleModifiedEventHandler> logger)
    : INotificationHandler<SaleModifiedEvent>
{
    public Task Handle(SaleModifiedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "[SaleModified] Sale {SaleNumber} (Id: {SaleId}) was modified at {OccurredOn}",
            notification.SaleNumber, notification.SaleId, notification.OccurredOn);

        return Task.CompletedTask;
    }
}

public class SaleCancelledEventHandler(ILogger<SaleCancelledEventHandler> logger)
    : INotificationHandler<SaleCancelledEvent>
{
    public Task Handle(SaleCancelledEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "[SaleCancelled] Sale {SaleNumber} (Id: {SaleId}) was cancelled at {OccurredOn}",
            notification.SaleNumber, notification.SaleId, notification.OccurredOn);

        return Task.CompletedTask;
    }
}

public class ItemCancelledEventHandler(ILogger<ItemCancelledEventHandler> logger)
    : INotificationHandler<ItemCancelledEvent>
{
    public Task Handle(ItemCancelledEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "[ItemCancelled] Item {ItemId} from Sale {SaleNumber} (Id: {SaleId}) was cancelled at {OccurredOn}",
            notification.ItemId, notification.SaleNumber, notification.SaleId, notification.OccurredOn);

        return Task.CompletedTask;
    }
}