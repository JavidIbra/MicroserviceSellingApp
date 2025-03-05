using EventBus.Base.Abstraction;
using Microsoft.Extensions.Logging;
using NotificationService.IntegrationEvents.Events;

namespace NotificationService.IntegrationEvents.EventHandlers
{
    internal sealed class OrderPaymentSuccessIntegrationEventHandler(ILogger<OrderPaymentSuccessIntegrationEventHandler> logger) : IIntegrationEventHandler<OrderPaymentSuccessIntegrationEvent>
    {
        public Task Handle(OrderPaymentSuccessIntegrationEvent @event)
        {
            // send success not : email sms push notification
            logger.LogInformation($"Order payment success with orderId {@event.OrderId}");

            return Task.CompletedTask;
        }
    }
}
