using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using AutoMapper;
using BittrexSharp.Domain;
using CsvHelper;
using CryptoGramBot.Models;
using Microsoft.Extensions.Logging;
using Poloniex.TradingTools;
using Poloniex.WalletTools;
using OpenOrder = CryptoGramBot.Models.OpenOrder;
using Order = Poloniex.TradingTools.Order;
using OrderType = Poloniex.General.OrderType;
using Trade = CryptoGramBot.Models.Trade;

namespace CryptoGramBot.Helpers
{
    public static class TradeConverter
    {
        public static List<Trade> BittrexFileToTrades(Stream csvExport, ILogger log)
        {
            TextReader fileReader = new StreamReader(csvExport, Encoding.Unicode);

            var csv = new CsvReader(fileReader);
            csv.Configuration.HasHeaderRecord = true;
            csv.Configuration.IsHeaderCaseSensitive = false;

            var tradeList = new List<HistoricOrder>();
            while (csv.Read())
            {
                var completedOrder = ConvertBittrexCsvToCompletedOrder(csv.CurrentRecord);
                tradeList.Add(completedOrder);
            }

            return BittrexToTrades(tradeList, log);
        }

        public static List<OpenOrder> BittrexToOpenOrders(IEnumerable<BittrexSharp.Domain.OpenOrder> bittrexOrders)
        {
            var list = new List<OpenOrder>();

            foreach (var openOrder in bittrexOrders)
            {
                var order = Mapper.Map<OpenOrder>(openOrder);
                order.Exchange = Constants.Bittrex;
                order.Price = openOrder.Limit;

                order.Side = openOrder.OrderType == "LIMIT_BUY" ? TradeSide.Buy : TradeSide.Sell;

                var ccy = openOrder.Exchange.Split('-');
                order.Base = ccy[0];
                order.Terms = ccy[1];

                list.Add(order);
            }

            return list;
        }

        public static List<Trade> BittrexToTrades(IEnumerable<HistoricOrder> bittrexTrades, ILogger logger)
        {
            var tradeList = new List<Trade>();

            foreach (var completedOrder in bittrexTrades)
            {
                var trade = Mapper.Map<Trade>(completedOrder);
                trade.Exchange = Constants.Bittrex;

                trade.Side = completedOrder.OrderType.Trim() == "LIMIT_BUY" ? TradeSide.Buy : TradeSide.Sell;

                if (trade.Side == TradeSide.Buy)
                {
                    trade.Cost = completedOrder.Price + completedOrder.Commission;
                }
                else if (trade.Side == TradeSide.Sell)
                {
                    trade.Cost = completedOrder.Price - completedOrder.Commission;
                }
                else
                {
                    logger.LogError($"SOMETHING NEEDS FIXING: TRADE SIDE IS {completedOrder.OrderType}");
                }

                var ccy = completedOrder.Exchange.Split('-');
                trade.Base = ccy[0];
                trade.Terms = ccy[1];

                tradeList.Add(trade);
            }

            return tradeList;
        }

        public static List<WalletBalance> BittrexToWalletBalances(IEnumerable<CurrencyBalance> response)
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
                    trade.Commission = Convert.ToDecimal((completedOrder.AmountQuote * 0.0025) *
                                                         completedOrder.PricePerCoin);
                    trade.Quantity = Convert.ToDecimal(completedOrder.AmountQuote - completedOrder.AmountQuote * 0.0025);
                    trade.Cost = baseAmount;
                }
                else
                {
                    trade.Commission = baseAmount * 0.0025m;
                    trade.Quantity = Convert.ToDecimal(completedOrder.AmountQuote);
                    trade.Cost = baseAmount - trade.Commission;
                }

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

        private static HistoricOrder ConvertBittrexCsvToCompletedOrder(IReadOnlyList<string> csvCurrentRecord)
        {
            var newOrder = new HistoricOrder()
            {
                OrderUuid = csvCurrentRecord[0],
                Exchange = csvCurrentRecord[1],
                Quantity = decimal.Parse(csvCurrentRecord[3]),
                Limit = decimal.Parse(csvCurrentRecord[4]),
                Commission = decimal.Parse(csvCurrentRecord[5]),
                Price = decimal.Parse(csvCurrentRecord[6]),
                Timestamp = DateTime.Parse(csvCurrentRecord[8], CultureInfo.CreateSpecificCulture("en-US"))
            };

            if (csvCurrentRecord[2] == "LIMIT_BUY")
            {
                newOrder.OrderType = BittrexSharp.Domain.OrderType.Buy;
            }
            else if (csvCurrentRecord[2] == "LIMIT_SELL")
            {
                newOrder.OrderType = BittrexSharp.Domain.OrderType.Sell;
            }

            return newOrder;
        }
    }
}