namespace OrderService.Domain.Models
{
    public class CustomerBasket
    {
        public CustomerBasket(string buyerId)
        {
            Items = new List<BasketItem>();
            BuyerId = buyerId;
        }

        public string BuyerId { get; set; }
        public List<BasketItem> Items { get; set; } = new List<BasketItem>();
    }
}
