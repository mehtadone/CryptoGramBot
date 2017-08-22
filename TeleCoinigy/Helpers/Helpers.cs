using System.Linq;
using Newtonsoft.Json.Linq;

namespace TeleCoinigy.Helpers
{
    public static class Helpers
    {
        public static double BalanceForAuthId(JObject jObject)
        {
            var data = jObject["data"];

            foreach (var token in data)
            {
                var balanceCcy = token["balance_curr_code"].ToString();
                if (balanceCcy == "BTC")
                {
                    var btcBalance = token["balance_amount_total"].ToString();
                    return double.Parse(btcBalance);
                }
            }

            return 0;
        }

        public static double TotalBtcBalance(JObject jObject)
        {
            double btcBalance = 0;
            btcBalance = jObject["data"]
                .Select(token => token["btc_balance"].ToString())
                .Select(double.Parse)
                .Aggregate(btcBalance, (current, d) => current + d);
            return btcBalance;
        }
    }
}
