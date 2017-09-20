using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using CryptoGramBot.EventBus.Events;
using CryptoGramBot.Helpers;
using CryptoGramBot.Services;
using Enexure.MicroBus;
using Microsoft.Extensions.Logging;

namespace CryptoGramBot.EventBus.Handlers
{
    public class PoloniexNewOrderCheckHandler : IEventHandler<NewTradesCheckEvent>
    {
        private readonly IMicroBus _bus;
        private readonly ILogger<PoloniexService> _log;
        private readonly PoloniexService _poloService;

        public PoloniexNewOrderCheckHandler(PoloniexService poloService, ILogger<PoloniexService> log, IMicroBus bus)
        {
            _poloService = poloService;
            _log = log;
            _bus = bus;
        }

        public async Task Handle(NewTradesCheckEvent @event)
        {
            try
            {
                var lastChecked = await _bus.QueryAsync(new LastCheckedQuery(Constants.Poloniex));
                var orderHistory = await _poloService.GetOrderHistory(lastChecked.LastChecked);
                var newTradesResponse = await _bus.QueryAsync(new FindNewTradeQuery(orderHistory));
                await _bus.SendAsync(new AddLastCheckedCommand(Constants.Poloniex));

                if (@event.PoloniexTradeNotification)
                {
                    var i = 0;
                    foreach (var newTrade in newTradesResponse.NewTrades)
                    {
                        if (@event.IsStartup && i > 4) break;
                        await _bus.SendAsync(new TradeNotificationCommand(newTrade));
                        i++;
                    }
                }
            }
            catch (Exception ex)
            {
                _log.LogError("Error in getting new orders from poloniex\n" + ex.Message);
                throw;
            }
        }
    }
}