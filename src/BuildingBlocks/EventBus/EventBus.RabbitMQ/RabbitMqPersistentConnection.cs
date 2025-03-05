using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using System.Net.Sockets;

namespace EventBus.RabbitMQ
{
    public class RabbitMqPersistentConnection : IDisposable
    {
        private readonly IConnectionFactory connectionFactory;
        private readonly int retryCount;
        private IConnection connection;
        private readonly object lock_object = new();
        private bool _disposed;

        public RabbitMqPersistentConnection(IConnectionFactory connectionFactory, int retryCount = 5)
        {
            this.connectionFactory = connectionFactory;
            this.retryCount = retryCount;
        }
        public bool IsConnected => connection != null && connection.IsOpen;

        public IChannel CreateChannel() => (IChannel)connection.CreateChannelAsync();

        public void Dispose()
        {
            _disposed = true;
            connection.Dispose();
        }

        public bool TryConnect()
        {
            lock (lock_object)
            {
                var policy = Policy.Handle<SocketException>()
                    .Or<BrokerUnreachableException>()
                    .WaitAndRetry(retryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time) =>
                    {

                    });

                policy.Execute(async () =>
                {
                    connection = await connectionFactory.CreateConnectionAsync();
                });

                if (IsConnected)
                {
                    connection.ConnectionShutdownAsync += Connection_ConnectionShutdownAsync; 
                    connection.CallbackExceptionAsync += Connection_CallbackExceptionAsync;
                    connection.ConnectionBlockedAsync += Connection_ConnectionBlockedAsync;

                    // log
                    return true;
                }
                return false;
            }
        }

        private Task Connection_ConnectionBlockedAsync(object sender, global::RabbitMQ.Client.Events.ConnectionBlockedEventArgs @event)
        {
            //if (_disposed) return;
            //TryConnect();

            if (_disposed) return Task.CompletedTask;

            throw new NotImplementedException();
        }

        private Task Connection_ConnectionShutdownAsync(object sender, global::RabbitMQ.Client.Events.ShutdownEventArgs @event)
        {
            //if (_disposed) return;
            //TryConnect();

            throw new NotImplementedException();
        }

        private Task Connection_CallbackExceptionAsync(object sender, global::RabbitMQ.Client.Events.CallbackExceptionEventArgs @event)
        {
            //if (_disposed) return;
            //TryConnect();

            throw new NotImplementedException();
        }
    }
}
