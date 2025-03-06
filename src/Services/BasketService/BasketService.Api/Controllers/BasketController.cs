using BasketService.Api.Core.Application.Repository;
using BasketService.Api.Core.Application.Services;
using BasketService.Api.Core.Domain.Models;
using BasketService.Api.IntegrationEvents.Events;
using EventBus.Base.Abstraction;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace BasketService.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BasketController : ControllerBase
    {
        private readonly IBasketRepository repository;
        private readonly IIdentityService identityService;
        private readonly IEventBus _eventBus;
        private readonly ILogger<BasketController> _logger;

        public BasketController(IBasketRepository repository, IIdentityService ıdentityService, IEventBus eventBus, ILogger<BasketController> logger)
        {
            this.repository = repository;
            this.identityService = ıdentityService;
            _eventBus = eventBus;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok("Basket Service is up and running");
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(CustomerBasket), (int)HttpStatusCode.OK)]
        public async Task<ActionResult> GetBasketByIdAsync(string id)
        {
            var basket = await repository.GetBasketAsync(id);
            return Ok(basket ?? new CustomerBasket(id));
        }

        [HttpPost()]
        [Route("update")]
        [ProducesResponseType(typeof(CustomerBasket), (int)HttpStatusCode.OK)]
        public async Task<ActionResult> UpdateBasketAsync([FromBody] CustomerBasket basket)
        {
            return Ok(await repository.UpdateBasketAsync(basket));
        }


        [HttpPost()]
        [Route("additem")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<ActionResult> AddItemToBasket([FromBody] BasketItem basketItem)
        {
            var userId = identityService.GetUserName().ToString();

            var basket = await repository.GetBasketAsync(userId);

            if (basket != null)
                basket = new CustomerBasket(userId);

            basket.Items.Add(basketItem);

            await repository.UpdateBasketAsync(basket);

            return Ok();
        }


        [HttpPost()]
        [Route("checkout")]
        [ProducesResponseType((int)HttpStatusCode.Accepted)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<ActionResult> CheckOutAsync([FromBody] BasketCheckout basketCheckout, [FromHeader(Name = "x-request-id")] string requestId)
        {
            var userId = basketCheckout.Buyer;

            var basket = await repository.GetBasketAsync(userId);

            if (basket is null)
                return BadRequest();

            var userName = identityService.GetUserName().ToString();

            var eventMessage = new OrderCreatedIntegrationEvent(userId, userName, basketCheckout.City, basketCheckout.Street, basketCheckout.State, basketCheckout.Country,
                                   basketCheckout.ZipCode, basketCheckout.CardNumber, basketCheckout.CardHolderName, basketCheckout.CardExpiration, basketCheckout.CardSecurityNumber, basketCheckout.CardtypeId
                                 , basketCheckout.Buyer, basket);

            try
            {
                // listen itself to clean the basket
                // it is listened by OrderApi to start the proccess
                _eventBus.Publish(eventMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when publishing event: {IntegrationEventId} from {BasketService.App}", eventMessage.Id);

                throw;  
            }

            return Accepted();
        }

    }
}
