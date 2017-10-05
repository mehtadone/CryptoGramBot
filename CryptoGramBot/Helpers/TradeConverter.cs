using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using AutoMapper;
using Bittrex;
using Bittrex.Data;
using CryptoGramBot.Helpers;
using CsvHelper;
using CryptoGramBot.Models;
using Poloniex.TradingTools;
using Poloniex.WalletTools;
using Remotion.Linq.Parsing.ExpressionVisitors.Transformation.PredefinedTransformations;
using OpenOrder = CryptoGramBot.Models.OpenOrder;
using OrderType = Poloniex.General.OrderType;
using Trade = CryptoGramBot.Models.Trade;

namespace CryptoGramBot.Helpers
{
    public static class TradeConverter
    {
        public static List<Trade> BittrexFileToTrades(Stream csvExport)
        {
            TextReader fileReader = new StreamReader(csvExport, Encoding.Unicode);

            var csv = new CsvReader(fileReader);
            csv.Configuration.HasHeaderRecord = true;
            csv.Configuration.IsHeaderCaseSensitive = false;

            var tradeList = new List<CompletedOrder>();
            while (csv.Read())
            {
                var completedOrder = ConvertBittrexCsvToCompletedOrder(csv.CurrentRecord);
                tradeList.Add(completedOrder);
            }

            return BittrexToTrades(tradeList);
        }

        public static List<OpenOrder> BittrexToOpenOrders(IEnumerable<Bittrex.OpenOrder> bittrexOrders)
        {
            var list = new List<OpenOrder>();

            foreach (var openOrder in bittrexOrders)
            {
                var order = Mapper.Map<OpenOrder>(openOrder);
                order.Exchange = Constants.Bittrex;
                order.Price = openOrder.Limit;

                order.Side = openOrder.OrderType == OpenOrderType.LIMIT_BUY ? TradeSide.Buy : TradeSide.Sell;

                var ccy = openOrder.Exchange.Split('-');
                order.Base = ccy[0];
                order.Terms = ccy[1];

                list.Add(order);
            }

            return list;
        }

        public static List<Trade> BittrexToTrades(IEnumerable<CompletedOrder> bittrexTrades)
        {
            var tradeList = new List<Trade>();

            foreach (var completedOrder in bittrexTrades)
            {
                var trade = Mapper.Map<Trade>(completedOrder);
                trade.Exchange = Constants.Bittrex;

                trade.Side = completedOrder.OrderType == OpenOrderType.LIMIT_BUY ? TradeSide.Buy : TradeSide.Sell;

                if (trade.Side == TradeSide.Buy)
                {
                    trade.Cost = completedOrder.Price + completedOrder.Commission;
                }
                else
                {
                    trade.Cost = completedOrder.Price - completedOrder.Commission;
                }

                var ccy = completedOrder.Exchange.Split('-');
                trade.Base = ccy[0];
                trade.Terms = ccy[1];

                tradeList.Add(trade);
            }

            return tradeList;
        }

        public static List<WalletBalance> BittrexToWalletBalances(GetBalancesResponse response)
        {
            var walletBalances = new List<WalletBalance>();

            foreach (var wallet in response)
            {
                if (wallet.Balance > 0)
                {
                    var walletBalance = Mapper.Map<WalletBalance>(wallet);
                    walletBalance.Exchange = Constants.Bittrex;
                    walletBalance.Timestamp = DateTime.Now;
                    walletBalances.Add(walletBalance);
                }
            }

            return walletBalances;
        }

        public static List<OpenOrder> PoloniexToOpenOrders(Dictionary<string, List<Order>> orders)
        {
            var openOrders = new List<OpenOrder>();

            foreach (var openOrderPair in orders)
            {
                foreach (var poloOrder in openOrderPair.Value)
                {
                    var openOrder = Mapper.Map<OpenOrder>(poloOrder);
                    openOrder.Exchange = Constants.Poloniex;
                    openOrder.Side = poloOrder.Type == OrderType.Buy ? TradeSide.Buy : TradeSide.Sell;

                    var ccy = openOrderPair.Key.Split('_');
                    openOrder.Base = ccy[0];
                    openOrder.Terms = ccy[1];

                    openOrder.Opened = DateTime.Now;

                    openOrder.Quantity = Convert.ToDecimal(poloOrder.AmountQuote);

                    openOrders.Add(openOrder);
                }
            }

            return openOrders;
        }

        public static List<Trade> PoloniexToTrades(IList<ITrade> trades, FeeInfo feeInfo)
        {
            var tradeList = new List<Trade>();

            foreach (var completedOrder in trades)
            {
                var trade = Mapper.Map<Trade>(completedOrder);
                trade.Exchange = Constants.Poloniex;
                trade.Side = completedOrder.Type == OrderType.Buy ? TradeSide.Buy : TradeSide.Sell;

                var ccy = completedOrder.Pair.Split('_');
                trade.Base = ccy[0];
                trade.Terms = ccy[1];

                var baseAmount = Convert.ToDecimal(completedOrder.AmountBase);

                if (trade.Side == TradeSide.Buy)
                {
                    trade.Cost = baseAmount;
                }
                else
                {
                    var commission = baseAmount * 0.0025m;
                    trade.Cost = baseAmount - commission;
                }

                trade.Quantity = Convert.ToDecimal(completedOrder.AmountQuote);

                tradeList.Add(trade);
            }

            return tradeList;
        }

        public static List<WalletBalance> PoloniexToWalletBalances(IDictionary<string, IBalance> balances)
        {
            var walletBalances = new List<WalletBalance>();

            foreach (var balance in balances)
            {
                if (balance.Value.BitcoinValue > 0)
                {
                    var walletBalance = new WalletBalance
                    {
                        Currency = balance.Key,
                        BtcAmount = Convert.ToDecimal(balance.Value.BitcoinValue),
                        Available = Convert.ToDecimal(balance.Value.QuoteAvailable),
                        Pending = Convert.ToDecimal(balance.Value.QuoteOnOrders),
                        Exchange = Constants.Poloniex,
                        Timestamp = DateTime.Now
                    };

                    walletBalances.Add(walletBalance);
                }
            }

            return walletBalances;
        }

        private static CompletedOrder ConvertBittrexCsvToCompletedOrder(IReadOnlyList<string> csvCurrentRecord)
        {
            var newOrder = new CompletedOrder
            {
                OrderUuid = csvCurrentRecord[0],
                Exchange = csvCurrentRecord[1],
                Quantity = decimal.Parse(csvCurrentRecord[3]),
                Limit = decimal.Parse(csvCurrentRecord[4]),
                Commission = decimal.Parse(csvCurrentRecord[5]),
                Price = decimal.Parse(csvCurrentRecord[6]),
                TimeStamp = DateTime.Parse(csvCurrentRecord[8], CultureInfo.CreateSpecificCulture("en-US"))
            };
            Enum.TryParse(csvCurrentRecord[2], true, out OpenOrderType orderSide);
            newOrder.OrderType = orderSide;

            return newOrder;
        }
    }
}