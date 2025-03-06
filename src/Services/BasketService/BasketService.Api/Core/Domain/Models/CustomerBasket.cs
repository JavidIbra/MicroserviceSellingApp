namespace BasketService.Api.Core.Domain.Models
{
    public class CustomerBasket
    {
        public CustomerBasket()
        {

        }
        public CustomerBasket(string buyerId)
        {
            BuyerId = buyerId;
        }

        public string BuyerId { get; set; }
        public List<BasketItem> Items { get; set; } = new List<BasketItem>();
    }
}
