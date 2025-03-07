using MediatR;

namespace OrderService.Domain.Events
{
    public class BuyerAndPaymentMethodVerifiedDomainEvent : INotification
    {
        public Buyer Buyer { get; private set; }
        public PaymentMethod Payment { get; private set; }
        public Guid OrderId{ get; private set; }

        public BuyerAndPaymentMethodVerifiedDomainEvent(Guid orderId, PaymentMethod payment, Buyer buyer)
        {
            OrderId = orderId;
            Payment = payment;
            Buyer = buyer;
        }
    }
}
