using BasketService.Api.Core.Application.Repository;
using BasketService.Api.Core.Application.Services;
using BasketService.Api.Extensions;
using BasketService.Api.Infrastructure.Repository;
using BasketService.Api.IntegrationEvents.EventHandlers;
using BasketService.Api.IntegrationEvents.Events;
using EventBus.Base;
using EventBus.Base.Abstraction;
using EventBus.Factory;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

ConfigureServicesExt(builder.Services);

builder.Services.AddScoped<IBasketRepository , BasketRepository>();
builder.Services.AddTransient<IIdentityService , IdentityService>();

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.RegisterWithConsul(app.Lifetime);

ConfigureSubscription(app.Services);

void ConfigureServicesExt(IServiceCollection serviceDescriptors)
{
    serviceDescriptors.ConfigureAuth(builder.Configuration);
    serviceDescriptors.AddSingleton(sp => sp.ConfigureRedis(builder.Configuration));

    serviceDescriptors.ConfigureConsul(builder.Configuration);

    serviceDescriptors.AddSingleton<IEventBus>(sp =>
    {
        EventBusConfig config = new()
        {
            ConnectionRetryCount = 5,
            EventNameSuffix = "IntegrationEvent",
            SubscriberCLientAppName = "BasketService",
            EventBusType = EventBusType.RabbitMQ,
        };

        return EventBusFactory.Create(config, sp);
    });

    serviceDescriptors.AddTransient<OrderCreatedIntegrationEventHandler>();
}

void ConfigureSubscription(IServiceProvider serviceProvider)
{
    var eventBus = serviceProvider.GetRequiredService<IEventBus>();

    eventBus.Subscribe<OrderCreatedIntegrationEvent, OrderCreatedIntegrationEventHandler>();    
}


app.MapControllers();

app.Run();
