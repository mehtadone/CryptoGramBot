using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CryptoGramBot.Models;
using Jojatekok.PoloniexAPI.TradingTools;
using Jojatekok.PoloniexAPI.WalletTools;
using Deposit = CryptoGramBot.Models.Deposit;
using Trade = CryptoGramBot.Models.Trade;
using Withdrawal = CryptoGramBot.Models.Withdrawal;

namespace CryptoGramBot.Helpers
{
    public class PoloniexConvertor
    {
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
                    Address = String.Empty
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
    }
}