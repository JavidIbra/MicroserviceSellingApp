using EventBus.Base.Events;

namespace NotificationService.IntegrationEvents.Events
{
    public class OrderPaymentFailedIntegrationEvent(int orderId, string errorMessage) : IntegrationEvent
    {
        public int OrderId { get; } = orderId;
        public string ErrorMessage { get; } = errorMessage;
    }
}
