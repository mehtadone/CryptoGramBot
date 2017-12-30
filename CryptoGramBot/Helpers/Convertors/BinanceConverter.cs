using System;
using System.Collections.Generic;
using System.Linq;
using Binance;
using Binance.Account;
using Binance.Account.Orders;
using CryptoGramBot.Models;
using Microsoft.Extensions.Logging;
using Deposit = CryptoGramBot.Models.Deposit;
using OpenOrder = CryptoGramBot.Models.OpenOrder;
using Trade = CryptoGramBot.Models.Trade;
using Withdrawal = CryptoGramBot.Models.Withdrawal;

namespace CryptoGramBot.Helpers.Convertors
{
    public static class BinanceConverter
    {
        public static List<Deposit> BinanceToDeposits(IEnumerable<Binance.Account.Deposit> binanceDesposits)
        {
            var list = new List<Deposit>();

            foreach (var exchangeDeposit in binanceDesposits)
            {
                var deposit = new Deposit
                {
                    Address = exchangeDeposit.Address,
                    Amount = Convert.ToDouble(exchangeDeposit.Amount),
                    Currency = exchangeDeposit.Asset,
                    Time = exchangeDeposit.Time(),
                    TransactionId = exchangeDeposit.TxId
                };

                list.Add(deposit);
            }

            return list;
        }

        public static List<OpenOrder> BinanceToOpenOrders(IEnumerable<Order> orderResponses, string baseCurrency, string termsCurrency)
        {
            var list = new List<OpenOrder>();

            foreach (var openOrder in orderResponses.Where(x => x.Status == OrderStatus.New))
            {
                var order = new OpenOrder
                {
                    Base = baseCurrency,
                    Terms = termsCurrency,
                    Exchange = Constants.Binance,
                    OrderUuid = openOrder.Id.ToString(),
                    Limit = openOrder.Price,
                    Opened = openOrder.Time(),
                    Price = openOrder.Price,
                    Quantity = openOrder.OriginalQuantity,
                    QuantityRemaining = openOrder.OriginalQuantity - openOrder.ExecutedQuantity,
                    Side = openOrder.Side == OrderSide.Buy ? TradeSide.Buy : TradeSide.Sell
                };

                list.Add(order);
            }

            return list;
        }

        public static List<Trade> BinanceToTrades(IEnumerable<AccountTrade> response, string baseCurrency, string termsCurrency, ILogger logger)
        {
            var tradeList = new List<Trade>();

            foreach (var completedOrder in response)
            {
                var trade = new Trade
                {
                    Exchange = Constants.Binance,
                    Base = baseCurrency,
                    Terms = termsCurrency,
                    Commission = completedOrder.Commission,
                    ExchangeId = completedOrder.Id.ToString(),
                    Limit = completedOrder.Price,
                    Quantity = completedOrder.Quantity,
                    QuantityRemaining = 0,
                    TimeStamp = completedOrder.Time(),
                    Side = completedOrder.IsBuyer ? TradeSide.Buy : TradeSide.Sell
                };

                trade.Cost = (completedOrder.Price * completedOrder.Quantity) * 0.999m;

                tradeList.Add(trade);
            }

            return tradeList;
        }

        public static List<WalletBalance> BinanceToWalletBalances(IEnumerable<AccountBalance> accountInfoBalances)
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

        public static List<Withdrawal> BinanceToWithdrawals(IEnumerable<Binance.Account.Withdrawal> binanceDespositsWithdrawList)
        {
            var list = new List<Withdrawal>();

            foreach (var exchangeDeposit in binanceDespositsWithdrawList)
            {
                var deposit = new Withdrawal
                {
                    Address = exchangeDeposit.Address,
                    Amount = Convert.ToDouble(exchangeDeposit.Amount),
                    Currency = exchangeDeposit.Asset,
                    Time = exchangeDeposit.Time(),
                    TransactionId = exchangeDeposit.TxId,
                };

                list.Add(deposit);
            }

            return list;
        }
    }
}