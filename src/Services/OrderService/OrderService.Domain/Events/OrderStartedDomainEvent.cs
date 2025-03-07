using MediatR;

namespace OrderService.Domain.Events
{
    public class OrderStartedDomainEvent : INotification
    {

        public OrderStartedDomainEvent(string userName, int cardTypeId, string cardNumber, string cardSecurityNumber, string cardHolderName, DateTime cardExpirationDate, Order order)
        {
            UserName = userName;
            CardTypeId = cardTypeId;
            CardNumber = cardNumber;
            CardSecurityNumber = cardSecurityNumber;
            CardHolderName = cardHolderName;
            CardExpirationDate = cardExpirationDate;
            Order = order;
        }
        public string UserName{ get;  }
        public int CardTypeId{ get;  }
        public string CardNumber{ get; }
        public string CardSecurityNumber{ get; }
        public string CardHolderName{ get;  }
        public DateTime CardExpirationDate{ get; }
        public Order Order{ get;  }
    }
}
