using EventBus.Base;
using EventBus.Base.Events;
using Newtonsoft.Json;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System.Net.Sockets;
using System.Text;

namespace EventBus.RabbitMQ
{
    public class EventBusRabbitMQ : BaseEventBus
    {
        RabbitMqPersistentConnection persistentConnection;
        private readonly IConnectionFactory connectionFactory;
        private readonly IChannel consumerChannel;

        public EventBusRabbitMQ(EventBusConfig config, IServiceProvider serviceProvider) : base(config, serviceProvider)
        {
            if (config.Connection != null)
            {
                var connJson = JsonConvert.SerializeObject(EventBusConfig.Connection, new JsonSerializerSettings()
                {
                    // self referencing loop detected for property
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                });

                connectionFactory = JsonConvert.DeserializeObject<ConnectionFactory>(connJson);
            }
            else
                connectionFactory = new ConnectionFactory();

            persistentConnection = new RabbitMqPersistentConnection(connectionFactory, config.ConnectionRetryCount);

            consumerChannel = CreateConsumerChannel();

            SubscriptionManager.OnEventRemoved += SubscriptionManager_OnEventRemoved; ;
        }

        private void SubscriptionManager_OnEventRemoved(object? sender, string eventName)
        {
            eventName = ProcessEventName(eventName);

            if (!persistentConnection.IsConnected)
            {
                persistentConnection.TryConnect();
            }

            consumerChannel.QueueUnbindAsync(queue: GetSubName(eventName),
                                            exchange: EventBusConfig.DefaultTopicName,
                                             routingKey: eventName
          );

            if (SubscriptionManager.IsEmpty)
            {
                consumerChannel.CloseAsync();
            }
        }

        public override async void Publish(IntegrationEvent @event)
        {
            if (!persistentConnection.IsConnected)
            {
                persistentConnection.TryConnect();
            }

            var policy = Policy.Handle<BrokerUnreachableException>()
                .Or<SocketException>()
                .WaitAndRetryAsync(EventBusConfig.ConnectionRetryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time) =>
                {
                    // Loglama
                });

            var eventName = @event.GetType().Name;
            eventName = ProcessEventName(eventName);

            await consumerChannel.ExchangeDeclareAsync(exchange: EventBusConfig.DefaultTopicName, type: "direct"); // ensure exchange exist when publishing

            var message = JsonConvert.SerializeObject(@event);
            var body = Encoding.UTF8.GetBytes(message);

            _ = policy.ExecuteAsync(async () =>
            {
                //var properties = consumerChannel;
                //properties.deliveryMood = 2; //  persistent

                await consumerChannel.QueueDeclareAsync(queue: GetSubName(eventName), // ensure queue exists while consuming
                                                     durable: true,
                                                         exclusive: false,
                                                              autoDelete: false,
                                                                     arguments: null
                        );

                await consumerChannel.BasicPublishAsync(
                  exchange: EventBusConfig.DefaultTopicName,
                  routingKey: eventName,
                  mandatory: true,
                  //basicProperties: properties,
                  body: body
                  );

            });
        }

        public override void Subscribe<T, TH>()
        {
            var eventName = typeof(T).Name;
            eventName = ProcessEventName(eventName);

            if (!SubscriptionManager.HasSubscriptionForEvent(eventName))
            {
                if (!persistentConnection.IsConnected)
                {
                    persistentConnection.TryConnect();
                }

                consumerChannel.QueueDeclareAsync(queue: GetSubName(eventName), // ensure queue exists while consuming
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null
                    );

                consumerChannel.QueueBindAsync(queue: GetSubName(eventName),
                 exchange: EventBusConfig.DefaultTopicName,
                 routingKey: eventName
                 );
            }

            SubscriptionManager.AddSubscription<T, TH>();
            StartBasicConsume(eventName);
        }

        public override void UnSubscribe<T, TH>()
        {
            SubscriptionManager.RemoveSubscription<T, TH>();
        }

        private IChannel CreateConsumerChannel()
        {
            if (!persistentConnection.IsConnected)
            {
                persistentConnection.TryConnect();
            }

            var channel = persistentConnection.CreateChannel();

            channel.ExchangeDeclareAsync(exchange: EventBusConfig.DefaultTopicName, type: "direct");

            return channel;
        }

        private void StartBasicConsume(string eventName)
        {
            if (consumerChannel is not null)
            {
                var consumer = new AsyncEventingBasicConsumer(consumerChannel);

                consumer.ReceivedAsync += Consumer_ReceivedAsync;

                consumerChannel.BasicConsumeAsync(
                    queue: GetSubName(eventName),
                    autoAck: false,
                    consumer: consumer);
            }
        }

        private async Task Consumer_ReceivedAsync(object sender, BasicDeliverEventArgs @event)
        {
            var eventName = @event.RoutingKey;
            eventName = ProcessEventName(eventName);
            var message = Encoding.UTF8.GetString(@event.Body.Span);

            try
            {
                await ProcessEvent(eventName, message);
            }
            catch (Exception)
            {
                // logging
                throw;
            }


            consumerChannel.BasicAckAsync(@event.DeliveryTag, multiple: false);
        }
    }
}
