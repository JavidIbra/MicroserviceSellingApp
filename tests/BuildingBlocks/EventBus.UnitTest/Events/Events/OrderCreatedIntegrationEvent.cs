namespace EventBus.UnitTest.Events.Events
{
    public class OrderCreatedIntegrationEvent : EventBusBase.IntegrationEvent
    {
        public int Id { get; set; }

        public OrderCreatedIntegrationEvent(int id)
        {
            Id = id;
        }
    }
}
