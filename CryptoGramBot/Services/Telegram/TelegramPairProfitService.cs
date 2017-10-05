using System;
using System.Threading.Tasks;
using CryptoGramBot.EventBus.Handlers;
using Enexure.MicroBus;
using Microsoft.Extensions.Logging;

namespace CryptoGramBot.Services.Telegram
{
    public class TelegramPairProfitService
    {
        private readonly IMicroBus _bus;
        private readonly ILogger<TelegramPairProfitService> _log;

        public TelegramPairProfitService(ILogger<TelegramPairProfitService> log, IMicroBus bus)
        {
            _log = log;
            _bus = bus;
        }

        public async Task RequestedPairProfit()
        {
            await _bus.SendAsync(new SendMessageCommand("What pair do you want to find your profits on? eg BTC-DOGE"));
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
                await _bus.SendAsync(new SendMessageCommand($"Something went wrong. Probably because you entered in a dud currency or I have no trade details"));
            }
            return PairProfitState.Reset();
        }
    }
}