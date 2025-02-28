﻿using EventBus.Base;
using EventBus.Base.Events;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text;
namespace EventBus.AzureServiceBus
{
    public class EventBusServiceBus : BaseEventBus
    {
        private ITopicClient topicClient;
        private ManagementClient managementClient;
        private ILogger logger;

        public EventBusServiceBus(EventBusConfig eventBusConfig, IServiceProvider serviceProvider) : base(eventBusConfig, serviceProvider)
        {
            logger = serviceProvider.GetService(typeof(ILogger<EventBusServiceBus>)) as ILogger<EventBusServiceBus>;
            managementClient = new ManagementClient(eventBusConfig.EventBusConnectionString);
            topicClient = CreateTopicClient();
        }

        private ITopicClient CreateTopicClient()
        {
            if (topicClient is null || topicClient.IsClosedOrClosing)
                topicClient = new TopicClient(EventBusConfig.EventBusConnectionString, EventBusConfig.DefaultTopicName, RetryPolicy.Default);

            // ensure that topic already exist
            if (!managementClient.TopicExistsAsync(EventBusConfig.DefaultTopicName).GetAwaiter().GetResult())
                managementClient.CreateTopicAsync(EventBusConfig.DefaultTopicName).GetAwaiter().GetResult();

            return topicClient;

        }

        public override void Publish(IntegrationEvent @event)
        {
            var eventName = @event.GetType().Name; // example: OrderCreatedIntegrationEvent

            eventName = ProcessEventName(eventName); //  OrderCreated

            var eventStr = JsonConvert.SerializeObject(@event);
            var bodyArr = Encoding.UTF8.GetBytes(eventStr);

            var message = new Message()
            {
                MessageId = Guid.NewGuid().ToString(),
                Body = bodyArr,
                Label = eventName,
            };

            topicClient.SendAsync(message).GetAwaiter().GetResult();
        }

        public override void Subscribe<T, TH>()
        {
            var eventName = typeof(T).Name;
            eventName = ProcessEventName(eventName);

            if (!SubscriptionManager.HasSubscriptionForEvent(eventName))
            {
                var subsClient = CreateSubscriptionClientIfNotexist(eventName);

                RegisterSubscriptionClientMessageHandler(subsClient);
            }

            logger.LogInformation("Subscribing to event {EventName} with {EventHandler}", eventName, typeof(TH).Name);

            SubscriptionManager.AddSubscription<T, TH>();
        }

        public override void UnSubscribe<T, TH>()
        {
            var eventName = typeof(T).Name;

            try
            {
                // Subscription will be there but  we dont subscribe
                var subscriptionClient = CreateSubscriptionClientIfNotexist(eventName);

                subscriptionClient.RemoveRuleAsync(eventName).GetAwaiter().GetResult();
            }
            catch (MessagingEntityNotFoundException)
            {
                logger.LogWarning("The messaging entity {eventName} couldn be found", eventName);
            }

            logger.LogInformation("Unsubscribing from event {EventName}", eventName);

            SubscriptionManager.RemoveSubscription<T, TH>();

        }

        private void RegisterSubscriptionClientMessageHandler(ISubscriptionClient subscriptionClient)
        {
            subscriptionClient.RegisterMessageHandler(async (message, token) =>
            {
                var eventName = $"{message.Label}";
                var messageData = Encoding.UTF8.GetString(message.Body);

                // Complete the message so that it is not receivedagain.
                if (await ProcessEvent(ProcessEventName(eventName), messageData))
                    await subscriptionClient.CompleteAsync(message.SystemProperties.LockToken);
            }, new MessageHandlerOptions(ExceptionReceiveHandler) { MaxConcurrentCalls = 10, AutoComplete = false });
        }

        private Task ExceptionReceiveHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            var ex = exceptionReceivedEventArgs.Exception;
            var context = exceptionReceivedEventArgs.ExceptionReceivedContext;

            logger.LogError(ex, "Error Handling message {ExceptionMessage} - Context: {@ExceptionContext}", ex.Message, context);

            return Task.CompletedTask;
        }

        private ISubscriptionClient CreateSubscriptionClientIfNotexist(string eventName)
        {
            var subClient = CreateSubscriptionClient(eventName);

            var exist = managementClient.SubscriptionExistsAsync(EventBusConfig.DefaultTopicName, GetSubName(eventName)).GetAwaiter().GetResult();

            if (!exist)
            {
                managementClient.CreateSubscriptionAsync(EventBusConfig.DefaultTopicName, GetSubName(eventName)).GetAwaiter().GetResult();
                RemoveDefaultRule(subClient);
            }

            CreateRuleIfNotExist(ProcessEventName(eventName), subClient);

            return subClient;
        }

        private void CreateRuleIfNotExist(string eventName, ISubscriptionClient client)
        {
            bool ruleExist;

            try
            {
                var rule = managementClient.GetRuleAsync(EventBusConfig.DefaultTopicName, eventName, eventName).GetAwaiter().GetResult();
                ruleExist = rule != null;
            }
            catch (MessagingEntityNotFoundException)
            {
                ruleExist = false;
            }

            if (!ruleExist)
            {
                client.AddRuleAsync(new RuleDescription
                {
                    Filter = new CorrelationFilter { Label = eventName },
                    Name = eventName,
                }).GetAwaiter().GetResult();
            }
        }

        private void RemoveDefaultRule(SubscriptionClient client)
        {
            try
            {
                client.RemoveRuleAsync(RuleDescription.DefaultRuleName).GetAwaiter().GetResult();
            }
            catch (MessagingEntityNotFoundException)
            {

                logger.LogWarning("the messaging entity {DefaultRuleName} Couldnt found", RuleDescription.DefaultRuleName);
            }
        }

        private SubscriptionClient CreateSubscriptionClient(string eventName)
        {
            return new SubscriptionClient(EventBusConfig.EventBusConnectionString, EventBusConfig.DefaultTopicName, GetSubName(eventName));
        }

        public override void Dispose()
        {
            base.Dispose();

            topicClient.CloseAsync().GetAwaiter().GetResult();
            managementClient.CloseAsync().GetAwaiter().GetResult();  

            topicClient = null;
            managementClient = null;
        }
    }
}
