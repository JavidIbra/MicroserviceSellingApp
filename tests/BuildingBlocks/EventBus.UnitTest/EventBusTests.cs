using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EventBus.UnitTest
{
    [TestClass]
    public class EventBusTests
    {
        private ServiceCollection services;

        public EventBusTests(ServiceCollection services)
        {
            this.services = new ServiceCollection();
            services.AddLogging(config => config.AddConsole());
        }

        [TestMethod]
        public void subscribe_event_on_rabbitmq_test()
        {
            var sp = services.BuildServiceProvider();

        }
    }
}
