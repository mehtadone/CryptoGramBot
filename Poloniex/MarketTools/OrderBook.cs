using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Jojatekok.PoloniexAPI.MarketTools
{
    public class OrderBook : IOrderBook
    {
        [JsonProperty("bids")]
        private IList<string[]> BuyOrdersInternal {
            set { BuyOrders = ParseOrders(value); }
        }
        public IList<IOrder> BuyOrders { get; private set; }

        [JsonProperty("asks")]
        private IList<string[]> SellOrdersInternal {
            set { SellOrders = ParseOrders(value); }
        }
        public IList<IOrder> SellOrders { get; private set; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static IList<IOrder> ParseOrders(IList<string[]> orders)
        {
            var output = new List<IOrder>(orders.Count);
            for (var i = 0; i < orders.Count; i++) {
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
