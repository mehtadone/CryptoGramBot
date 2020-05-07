using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Bittrex.Net.Objects;
using CryptoGramBot.Models;
using CsvHelper;
using Microsoft.Extensions.Logging;

namespace CryptoGramBot.Helpers
{
    public class BittrexConvertor
    {
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

        public static List<OpenOrder> BittrexToOpenOrders(BittrexOpenOrdersOrder[] bittrexOrders)
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
                    Condition = openOrder.Condition.ToString(),
                    ConditionTarget = openOrder.ConditionTarget?.ToString(),
                    CancelInitiated = openOrder.CancelInitiated,
                    ImmediateOrCancel = openOrder.ImmediateOrCancel,
                    IsConditional = openOrder.IsConditional,
                    Limit = openOrder.Limit,
                    Opened = openOrder.Opened,
                    Price = openOrder.Price,
                    Quantity = openOrder.Quantity,
                    QuantityRemaining = openOrder.QuantityRemaining,
                    Side = openOrder.OrderType == OrderSideExtended.LimitBuy ? TradeSide.Buy : TradeSide.Sell
                };

                list.Add(order);
            }

            return list;
        }

        public static List<Trade> BittrexToTrades(BittrexOrderHistoryOrder[] bittrexTrades, ILogger logger)
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
                    Commission = completedOrder.Commission,
                    ExchangeId = completedOrder.OrderUuid.ToString(),
                    Limit = completedOrder.Limit,
                    Quantity = completedOrder.Quantity,
                    QuantityRemaining = completedOrder.QuantityRemaining,
                    Timestamp = completedOrder.Closed.GetValueOrDefault(),
                    Side = completedOrder.OrderType == OrderSideExtended.LimitBuy ? TradeSide.Buy : TradeSide.Sell
                };

                if (completedOrder.Closed.HasValue)

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
                        Available = wallet.Available ?? 0,
                        Balance = wallet.Balance ?? 0,
                        Pending = wallet.Pending ?? 0,
                        Currency = wallet.Currency,
                    };

                    if (String.IsNullOrEmpty(wallet.CryptoAddress))
                    {
                        walletBalance.Address = String.Empty;
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

        private static Trade ConvertBittrexCsvToCompletedOrder(IReadOnlyList<string> csvCurrentRecord)
        {
            var ccy = csvCurrentRecord[1].Split('-');
            var newOrder = new Trade()
            {
                Exchange = Constants.Bittrex,
                Base = ccy[0],
                Terms = ccy[1],
                Commission = Decimal.Parse(csvCurrentRecord[5], CultureInfo.CreateSpecificCulture("en-US")),
                ExchangeId = csvCurrentRecord[0],
                Limit = Decimal.Parse(csvCurrentRecord[4], CultureInfo.CreateSpecificCulture("en-US")),
                Quantity = Decimal.Parse(csvCurrentRecord[3], CultureInfo.CreateSpecificCulture("en-US")),
                Cost = Decimal.Parse(csvCurrentRecord[6], CultureInfo.CreateSpecificCulture("en-US")) * Decimal.Parse(csvCurrentRecord[3], CultureInfo.CreateSpecificCulture("en-US")),
                Timestamp = DateTime.Parse(csvCurrentRecord[8], CultureInfo.CreateSpecificCulture("en-US"))
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
