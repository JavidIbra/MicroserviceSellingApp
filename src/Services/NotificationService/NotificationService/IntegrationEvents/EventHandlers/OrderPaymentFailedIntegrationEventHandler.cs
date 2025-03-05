using EventBus.Base.Abstraction;
using Microsoft.Extensions.Logging;
using NotificationService.IntegrationEvents.Events;

namespace NotificationService.IntegrationEvents.EventHandlers
{
    internal sealed class OrderPaymentFailedIntegrationEventHandler(ILogger<OrderPaymentFailedIntegrationEventHandler> logger) : IIntegrationEventHandler<OrderPaymentFailedIntegrationEvent>
    {
        public Task Handle(OrderPaymentFailedIntegrationEvent @event)
        {

            // send fail : email sms push notification
            logger.LogInformation($"Order payment failed with orderId {@event.OrderId}, errorMessage {@event.ErrorMessage}");

            return Task.CompletedTask;
        }
    }
}
