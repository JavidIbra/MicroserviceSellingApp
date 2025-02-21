using EventBus.Base.Abstraction;
using EventBus.Base.SubManagers;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace EventBus.Base.Events
{
    public abstract class BaseEventBus : IEventBus
    {
        public readonly IServiceProvider ServiceProvider;
        public readonly IEventBusSubscriptionManager SubscriptionManager;

        public EventBusConfig EventBusConfig { get; set; }

        protected BaseEventBus(EventBusConfig eventBusConfig, IServiceProvider serviceProvider)
        {
            EventBusConfig = eventBusConfig;
            ServiceProvider = serviceProvider;
            SubscriptionManager = new InMemoryEventBusSubscriptionManager(ProcessEventName);
        }

        public virtual string ProcessEventName(string eventName)
        {
            if (EventBusConfig.DeleteEventPrefix)
                eventName = eventName.TrimStart([.. EventBusConfig.EventNamePrefix]);

            if (EventBusConfig.DeleteEventSuffix)
                eventName = eventName.TrimStart([.. EventBusConfig.EventNameSuffix]);

            return eventName;
        }

        public virtual string GetSubName(string eventName)
        {
            return $"{EventBusConfig.SubscriberCLientAppName}.{ProcessEventName(eventName)}";
        }

        public virtual void Dispose() { EventBusConfig = null; SubscriptionManager.Clear(); }


        public async Task<bool> ProcessEvent(string eventName, string message)
        {
            eventName = ProcessEventName(eventName);
            var processed = false;

            if (SubscriptionManager.HasSubscriptionForEvent(eventName))
            {
                var subscriptions = SubscriptionManager.GetHandlersForEvent(eventName);

                using (var scope = ServiceProvider.CreateScope())
                {

                    foreach (var subscription in subscriptions)
                    {
                        var handler = ServiceProvider.GetService(subscription.HandlerType);
                        if (handler == null) continue;

                        var eventType = SubscriptionManager.GetEventtypeByName($"{EventBusConfig.EventNamePrefix}{eventName}{EventBusConfig.EventNameSuffix}");
                        var integrationEvent = JsonConvert.DeserializeObject(message, eventType);

                        var concreteType = typeof(IIntegrationEventHandler<>).MakeGenericType(eventType);
                        await (Task)concreteType.GetMethod("Handle").Invoke(handler, new object[] { integrationEvent });
                    }
                }

                processed = true;
            }

            return processed;
        }

        public abstract void Publish(IntegrationEvent @event);

        public abstract void Subscribe<T, TH>()
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>;

        public abstract void UnSubscribe<T, TH>()
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>;
    }
}
