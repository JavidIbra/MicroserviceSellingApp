﻿using BasketService.Api.Core.Application.Repository;
using BasketService.Api.IntegrationEvents.Events;
using EventBus.Base.Abstraction;

namespace BasketService.Api.IntegrationEvents.EventHandlers
{
    public class OrderCreatedIntegrationEventHandler : IIntegrationEventHandler<OrderCreatedIntegrationEvent>
    {
        private readonly IBasketRepository _repository;
        private readonly ILogger<OrderCreatedIntegrationEvent> _logger;

        public OrderCreatedIntegrationEventHandler(ILogger<OrderCreatedIntegrationEvent> logger, IBasketRepository repository)
        {
            _logger = logger;
            _repository = repository;
        }

        public async Task Handle(OrderCreatedIntegrationEvent @event)
        {
                
            _logger.LogInformation("----- Handling integration event: {IntegrationEventId} at BasketService.Api - ({@IntegrationEvent}) ", @event.Id);
            await _repository.DeletBasketAsync(@event.UserId.ToString());

        }
    }
}
