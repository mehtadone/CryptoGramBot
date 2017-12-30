using System;
using System.Threading.Tasks;
using CryptoGramBot.EventBus.Handlers;
using CryptoGramBot.Helpers;
using Enexure.MicroBus;
using Microsoft.Extensions.Logging;

namespace CryptoGramBot.Services.Telegram
{
    public class TelegramPairProfitService : IDisposable
    {
        private readonly IMicroBus _bus;
        private readonly ILogger<TelegramPairProfitService> _log;

        public TelegramPairProfitService(ILogger<TelegramPairProfitService> log, IMicroBus bus)
        {
            _log = log;
            _bus = bus;
        }

        public void Dispose()
        {
        }

        public async Task RequestedPairProfit()
        {
            var mess = new StringBuffer();
            mess.Append(StringContants.WhatPairProfits);
            await _bus.SendAsync(new SendMessageCommand(mess));
            PairProfitState.WaitingForCurrency = true;
        }

        public async Task<bool> SendPairProfit(string currency)
        {
            if (!PairProfitState.WaitingForCurrency) return false;

            try
            {
                var ccy = currency.Trim().ToUpper();
                _log.LogInformation($"User wants to check for profit for {ccy}");
                await _bus.SendAsync(new PairProfitCommand(ccy));
            }
            catch (Exception)
            {
                var mess = new StringBuffer();
                mess.Append(StringContants.PairProfitError);
                await _bus.SendAsync(new SendMessageCommand(mess));
            }
            return PairProfitState.Reset();
        }
    }
}