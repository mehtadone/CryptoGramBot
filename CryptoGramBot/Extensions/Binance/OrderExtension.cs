namespace Binance.Account.Orders
{
    public static class OrderExtension
    {
        public static bool IsOpen(this Order order)
        {
            if(order == null)
            {
                return false;
            }

            return order.Status == OrderStatus.New || order.Status == OrderStatus.PartiallyFilled;
        }
    }
}
