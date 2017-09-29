using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using AutoMapper;
using Bittrex;
using Bittrex.Data;
using CsvHelper;
using CryptoGramBot.Models;
using Poloniex.TradingTools;
using Poloniex.WalletTools;
using ITrade = Poloniex.TradingTools.ITrade;
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
                    walletBalances.Add(walletBalance);
                }
            }

            return walletBalances;
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
                    trade.Cost = baseAmount + (baseAmount * 0.025m);
                }
                else
                {
                    trade.Cost = baseAmount - (baseAmount * 0.025m);
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
                        Exchange = Constants.Poloniex
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