using Consul;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http.Features;

namespace BasketService.Api.Extensions
{
    public static class ConsulRegistration
    {
        public static IServiceCollection ConfigureConsul(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IConsulClient, ConsulClient>(p => new ConsulClient(conf =>
            {
                var address = configuration["ConsulConfig:Address"];
                conf.Address = new Uri(address);
            }));

            return services;
        }

        public static IApplicationBuilder RegisterWithConsul(this IApplicationBuilder applicationBuilder, IHostApplicationLifetime lifetime)
        {

            var consulClient = applicationBuilder.ApplicationServices.GetRequiredService<IConsulClient>();
            var loggingFactory = applicationBuilder.ApplicationServices.GetRequiredService<ILoggerFactory>();
            var logger = loggingFactory.CreateLogger<IApplicationBuilder>();

            // Get Server Ip Adress
            var features = applicationBuilder.Properties["server.Features"] as FeatureCollection;
            var addresses = features?.Get<IServerAddressesFeature>();
            var address = addresses?.Addresses.FirstOrDefault();

            // Register Services with consul
            var uri = new Uri(address);
            var registration = new AgentServiceRegistration()
            {
                ID = $"BasketService",
                Name = "BasketService",
                Address = $"{uri.Host}",
                Port = uri.Port,
                Tags = new[] { "Basket Service", "Basket" }
            };

            logger.LogInformation("Registering with Consul");

            consulClient.Agent.ServiceDeregister(registration.ID).Wait();
            consulClient.Agent.ServiceRegister(registration).Wait();

            lifetime.ApplicationStopping.Register(() =>
            {
                logger.LogInformation("DeRegistering from Consul");
                consulClient.Agent.ServiceDeregister(registration.ID).Wait();

            });

            return applicationBuilder;
        }

    }
}
