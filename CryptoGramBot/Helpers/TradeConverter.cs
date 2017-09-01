using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using AutoMapper;
using Bittrex;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using CryptoGramBot.Models;

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

                var ccy = completedOrder.Exchange.Split('-');
                trade.Base = ccy[0];
                trade.Terms = ccy[1];

                tradeList.Add(trade);
            }

            return tradeList;
        }

        private static CompletedOrder ConvertBittrexCsvToCompletedOrder(string[] csvCurrentRecord)
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
            OpenOrderType orderSide;
            Enum.TryParse(csvCurrentRecord[2], true, out orderSide);
            newOrder.OrderType = orderSide;

            return newOrder;
        }
    }
}