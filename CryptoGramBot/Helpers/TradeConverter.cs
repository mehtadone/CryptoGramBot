using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using BinanceExchange.API.Models.Response;
using Bittrex.Net.Objects;
using CsvHelper;
using CryptoGramBot.Models;
using Jojatekok.PoloniexAPI.TradingTools;
using Jojatekok.PoloniexAPI.WalletTools;
using Microsoft.Extensions.Logging;
using Poloniex.TradingTools;
using Deposit = CryptoGramBot.Models.Deposit;
using OpenOrder = CryptoGramBot.Models.OpenOrder;
using Trade = CryptoGramBot.Models.Trade;
using Withdrawal = CryptoGramBot.Models.Withdrawal;

namespace CryptoGramBot.Helpers
{
    public static class TradeConverter
    {
        public static List<WalletBalance> BinanceToWalletBalances(List<BalanceResponse> accountInfoBalances)
        {
            var walletBalances = new List<WalletBalance>();

            foreach (var balance in accountInfoBalances)
            {
                if (balance.Locked + balance.Free > 0)
                {
                    var walletBalance = new WalletBalance
                    {
                        Currency = balance.Asset,
                        //                        BtcAmount = Convert.ToDecimal(balance.Value.BitcoinValue),
                        Available = balance.Free,
                        Balance = balance.Free + balance.Locked,
                        Pending = balance.Locked,
                        Exchange = Constants.Binance,
                        Timestamp = DateTime.Now
                    };

                    walletBalances.Add(walletBalance);
                }
            }

            return walletBalances;
        }

        public static List<Trade> BittrexFileToTrades(Stream csvExport, ILogger log)
        {
            TextReader fileReader = new StreamReader(csvExport, Encoding.Unicode);

            var csv = new CsvReader(fileReader);
            csv.Configuration.HasHeaderRecord = true;
            csv.Configuration.IsHeaderCaseSensitive = false;

            var tradeList = new List<Trade>();
            while (csv.Read())
            {
                var completedOrder = ConvertBittrexCsvToCompletedOrder(csv.CurrentRecord);
                tradeList.Add(completedOrder);
            }

            return tradeList;
        }

        public static List<Deposit> BittrexToDeposits(BittrexDeposit[] responseResult)
        {
            var list = new List<Deposit>();

            foreach (var exchangeDeposit in responseResult)
            {
                var deposit = new Deposit
                {
                    Address = exchangeDeposit.CryptoAddress,
                    Amount = Convert.ToDouble(exchangeDeposit.Amount),
                    Confirmations = (uint)exchangeDeposit.Confirmations,
                    Currency = exchangeDeposit.Currency,
                    Time = exchangeDeposit.LastUpdated,
                    TransactionId = exchangeDeposit.TransactionId
                };

                list.Add(deposit);
            }

            return list;
        }

        public static List<OpenOrder> BittrexToOpenOrders(BittrexOrder[] bittrexOrders)
        {
            var list = new List<OpenOrder>();

            foreach (var openOrder in bittrexOrders)
            {
                var pair = openOrder.Exchange.Split("-");
                var order = new OpenOrder
                {
                    Base = pair[0],
                    Terms = pair[1],
                    Exchange = Constants.Bittrex,
                    CommissionPaid = openOrder.CommissionPaid,
                    OrderUuid = openOrder.OrderUuid.ToString(),
                    Condition = openOrder.Condition,
                    ConditionTarget = openOrder.ConditionTarget,
                    CancelInitiated = openOrder.CancelInitiated,
                    ImmediateOrCancel = openOrder.ImmediateOrCancel,
                    IsConditional = openOrder.IsConditional,
                    Limit = openOrder.Limit,
                    Opened = openOrder.Opened,
                    Price = openOrder.Price,
                    Quantity = openOrder.Quantity,
                    QuantityRemaining = openOrder.QuantityRemaining,
                    Side = openOrder.OrderType == OrderTypeExtended.LimitBuy ? TradeSide.Buy : TradeSide.Sell
                };

                list.Add(order);
            }

            return list;
        }

        public static List<Trade> BittrexToTrades(BittrexOrder[] bittrexTrades, ILogger logger)
        {
            var tradeList = new List<Trade>();

            foreach (var completedOrder in bittrexTrades)
            {
                var ccy = completedOrder.Exchange.Split('-');
                var trade = new Trade
                {
                    Exchange = Constants.Bittrex,
                    Base = ccy[0],
                    Terms = ccy[1],
                    Commission = completedOrder.CommissionPaid,
                    ExchangeId = completedOrder.OrderUuid.ToString(),
                    Limit = completedOrder.Limit,
                    Quantity = completedOrder.Quantity,
                    QuantityRemaining = completedOrder.QuantityRemaining,
                    TimeStamp = completedOrder.Closed.GetValueOrDefault(),
                    Side = completedOrder.OrderType == OrderTypeExtended.LimitBuy ? TradeSide.Buy : TradeSide.Sell
                };

                if (completedOrder.Closed.HasValue)

                    if (trade.Side == TradeSide.Buy)
                    {
                        trade.Cost = completedOrder.Price + completedOrder.CommissionPaid;
                    }
                    else if (trade.Side == TradeSide.Sell)
                    {
                        trade.Cost = completedOrder.Price - completedOrder.CommissionPaid;
                    }
                    else
                    {
                        logger.LogError($"SOMETHING NEEDS FIXING: TRADE SIDE IS {completedOrder.OrderType}");
                    }

                tradeList.Add(trade);
            }

            return tradeList;
        }

        public static List<WalletBalance> BittrexToWalletBalances(BittrexBalance[] response)
        {
            var walletBalances = new List<WalletBalance>();

            foreach (var wallet in response)
            {
                if (wallet.Balance > 0)
                {
                    var walletBalance = new WalletBalance
                    {
                        Exchange = Constants.Bittrex,
                        Timestamp = DateTime.Now,
                        Address = wallet.CryptoAddress,
                        Available = wallet.Available,
                        Balance = wallet.Balance,
                        Pending = wallet.Pending,
                        Currency = wallet.Currency,
                        Requested = wallet.Requested,
                        Uuid = wallet.Uuid
                    };

                    if (string.IsNullOrEmpty(wallet.CryptoAddress))
                    {
                        walletBalance.Address = string.Empty;
                    }

                    walletBalances.Add(walletBalance);
                }
            }

            return walletBalances;
        }

        public static List<Withdrawal> BittrexToWithdrawals(BittrexWithdrawal[] responseResult)
        {
            var list = new List<Withdrawal>();

            foreach (var exchangeDeposit in responseResult)
            {
                var deposit = new Withdrawal
                {
                    Address = exchangeDeposit.Address,
                    Amount = Convert.ToDouble(exchangeDeposit.Amount),
                    Cost = Convert.ToDouble(exchangeDeposit.TransactionCost),
                    Currency = exchangeDeposit.Currency,
                    Time = exchangeDeposit.Opened,
                    TransactionId = exchangeDeposit.TransactionId,
                };

                list.Add(deposit);
            }

            return list;
        }

        public static List<Deposit> PoloniexToDeposits(IList<Jojatekok.PoloniexAPI.WalletTools.Deposit> poloDeposits)
        {
            var list = new List<Deposit>();

            foreach (var exchangeDeposit in poloDeposits)
            {
                var deposit = new Deposit
                {
                    Address = exchangeDeposit.Address,
                    Amount = Convert.ToDouble(exchangeDeposit.Amount),
                    Confirmations = exchangeDeposit.Confirmations,
                    Currency = exchangeDeposit.Currency,
                    Time = exchangeDeposit.Time,
                    TransactionId = exchangeDeposit.TransactionId,
                };

                list.Add(deposit);
            }

            return list;
        }

        public static List<OpenOrder> PoloniexToOpenOrders(Dictionary<string, List<Order>> orders)
        {
            var list = new List<OpenOrder>();

            foreach (var openOrderPair in orders)
            {
                foreach (var openOrder in openOrderPair.Value)
                {
                    var ccy = openOrderPair.Key.Split('_');
                    var order = new OpenOrder
                    {
                        Exchange = Constants.Poloniex,
                        Side = openOrder.Type == Jojatekok.PoloniexAPI.OrderType.Buy ? TradeSide.Buy : TradeSide.Sell,
                        Base = ccy[0],
                        Terms = ccy[1],
                        Opened = DateTime.Now,
                        Quantity = Convert.ToDecimal(openOrder.AmountQuote),
                        Price = Convert.ToDecimal(openOrder.PricePerCoin),
                        OrderUuid = openOrder.IdOrder.ToString()
                    };

                    list.Add(order);
                }
            }

            return list;
        }

        public static List<Trade> PoloniexToTrades(IList<ITrade> trades)
        {
            var tradeList = new List<Trade>();

            foreach (var completedOrder in trades)
            {
                var ccy = completedOrder.Pair.Split('_');
                var trade = new Trade
                {
                    Exchange = Constants.Poloniex,
                    Side = completedOrder.Type == Jojatekok.PoloniexAPI.OrderType.Buy ? TradeSide.Buy : TradeSide.Sell,
                    Base = ccy[0],
                    Terms = ccy[1],
                    ExchangeId = completedOrder.IdOrder.ToString(),
                    Limit = Convert.ToDecimal(completedOrder.PricePerCoin),
                    TimeStamp = completedOrder.Time,
                };

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

        public static List<WalletBalance> PoloniexToWalletBalances(IDictionary<string, Balance> balances)
        {
            var walletBalances = new List<WalletBalance>();

            foreach (var balance in balances)
            {
                if (!(balance.Value.BitcoinValue > 0)) continue;

                var walletBalance = new WalletBalance
                {
                    Currency = balance.Key,
                    BtcAmount = Convert.ToDecimal(balance.Value.BitcoinValue),
                    Available = Convert.ToDecimal(balance.Value.QuoteAvailable),
                    Balance = Convert.ToDecimal(balance.Value.QuoteAvailable),
                    Pending = Convert.ToDecimal(balance.Value.QuoteOnOrders),
                    Exchange = Constants.Poloniex,
                    Timestamp = DateTime.Now,
                    Address = string.Empty
                };

                walletBalances.Add(walletBalance);
            }

            return walletBalances;
        }

        public static List<Withdrawal> PoloniexToWithdrawals(IList<Jojatekok.PoloniexAPI.WalletTools.Withdrawal> poloWithdrawals)
        {
            var list = new List<Withdrawal>();

            foreach (var exchangeDeposit in poloWithdrawals)
            {
                var deposit = new Withdrawal
                {
                    Address = exchangeDeposit.Address,
                    Amount = Convert.ToDouble(exchangeDeposit.Amount),
                    Currency = exchangeDeposit.Currency,
                    Time = exchangeDeposit.Time,
                    TransactionId = exchangeDeposit.Id.ToString(),
                    IpAddress = exchangeDeposit.IpAddress
                };

                list.Add(deposit);
            }

            return list;
        }

        private static Trade ConvertBittrexCsvToCompletedOrder(IReadOnlyList<string> csvCurrentRecord)
        {
            var newOrder = new Trade()
            {
                ExchangeId = csvCurrentRecord[0],
                Exchange = csvCurrentRecord[1],
                Quantity = decimal.Parse(csvCurrentRecord[3]),
                Limit = decimal.Parse(csvCurrentRecord[4]),
                Commission = decimal.Parse(csvCurrentRecord[5]),
                Cost = decimal.Parse(csvCurrentRecord[6]) * decimal.Parse(csvCurrentRecord[3]),
                TimeStamp = DateTime.Parse(csvCurrentRecord[8], CultureInfo.CreateSpecificCulture("en-US"))
            };

            if (csvCurrentRecord[2] == "LIMIT_BUY")
            {
                newOrder.Side = TradeSide.Buy;
            }
            else if (csvCurrentRecord[2] == "LIMIT_SELL")
            {
                newOrder.Side = TradeSide.Sell;
            }

            return newOrder;
        }
    }
}