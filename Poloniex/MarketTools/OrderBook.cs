using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Jojatekok.PoloniexAPI;
using Newtonsoft.Json;

namespace Poloniex.MarketTools
{
    public class OrderBook : IOrderBook
    {
        public IList<IOrder> BuyOrders { get; private set; }

        public IList<IOrder> SellOrders { get; private set; }

        [JsonProperty("bids")]
        private IList<string[]> BuyOrdersInternal
        {
            set { BuyOrders = ParseOrders(value); }
        }

        [JsonProperty("asks")]
        private IList<string[]> SellOrdersInternal
        {
            set { SellOrders = ParseOrders(value); }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static IList<IOrder> ParseOrders(IList<string[]> orders)
        {
            var output = new List<IOrder>(orders.Count);
            for (var i = 0; i < orders.Count; i++)
            {
                output.Add(
                    new Order(
                        double.Parse(orders[i][0], Helper.InvariantCulture),
                        double.Parse(orders[i][1], Helper.InvariantCulture)
                    )
                );
            }
            return output;
        }
    }
}