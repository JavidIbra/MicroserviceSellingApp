using EventBus.Base;
using EventBus.Base.Abstraction;
using EventBus.Factory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NotificationService.IntegrationEvents.EventHandlers;
using NotificationService.IntegrationEvents.Events;


ServiceCollection coll = new ServiceCollection();

ConfigureService(coll);

var sp = coll.BuildServiceProvider();

IEventBus eventBus = sp.GetRequiredService<IEventBus>();

eventBus.Subscribe<OrderPaymentSuccessIntegrationEvent, OrderPaymentSuccessIntegrationEventHandler>();
eventBus.Subscribe<OrderPaymentFailedIntegrationEvent, OrderPaymentFailedIntegrationEventHandler>();

Console.WriteLine("Application is running, World!");
Console.ReadLine();

static void ConfigureService(ServiceCollection services)
{
    services.AddLogging(conf => { conf.AddConsole(); });

    services.AddTransient<OrderPaymentFailedIntegrationEventHandler>();
    services.AddTransient<OrderPaymentSuccessIntegrationEventHandler>();

    services.AddSingleton<IEventBus>(sp =>
    {
        EventBusConfig config = new()
        {
            ConnectionRetryCount = 5,
            EventNameSuffix = "IntegrationEvent",
            SubscriberCLientAppName = "NotificationService",
            EventBusType = EventBusType.RabbitMQ,
        };

        return EventBusFactory.Create(config, sp);
    });
}

