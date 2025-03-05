using EventBus.Base.Events;

namespace NotificationService.IntegrationEvents.Events
{
    public class OrderPaymentSuccessIntegrationEvent(int orderId) : IntegrationEvent
    {
        public int OrderId { get; } = orderId;
    }
}
