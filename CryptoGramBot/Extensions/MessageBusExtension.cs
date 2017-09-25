using CryptoGramBot.Configuration;
using CryptoGramBot.EventBus;
using CryptoGramBot.EventBus.Commands;
using CryptoGramBot.EventBus.Events;
using CryptoGramBot.EventBus.Handlers;
using CryptoGramBot.EventBus.Handlers.BalanceInfo;
using CryptoGramBot.Services;
using Enexure.MicroBus;

namespace CryptoGramBot.Extensions
{
    public static class MessageBusExtension
    {
        public static BusBuilder ConfigureCore(this BusBuilder busBuilder, bool coinigyEnabled, bool bittrexEnabled, bool poloniexEnabled, bool bagEnabled, bool dustEnabled)
        {
            busBuilder.RegisterCommandHandler<SendMessageCommand, SendMessageHandler>();
            busBuilder.RegisterCommandHandler<SendFileCommand, SendFileHandler>();
            busBuilder.RegisterCommandHandler<SendHelpMessageCommand, HelpMessageHandler>();
            busBuilder.RegisterCommandHandler<PairProfitCommand, PairProfitHandler>();
            busBuilder.RegisterCommandHandler<ExcelExportCommand, ExcelExportHandler>();
            busBuilder.RegisterCommandHandler<TradeNotificationCommand, TradeNotificationHandler>();
            busBuilder.RegisterQueryHandler<LastCheckedQuery, LastCheckedResponse, LastCheckedHandler>();
            busBuilder.RegisterQueryHandler<FindNewTradeQuery, FindNewTradesResponse, SaveAndFindNewTradesHandler>();
            busBuilder.RegisterQueryHandler<TradeProfitQuery, TradesProfitResponse, TradeProfitHandler>();
            busBuilder.RegisterCommandHandler<AddLastCheckedCommand, AddLastCheckedHandler>();
            busBuilder.RegisterCommandHandler<SendBalanceInfoCommand, SendBalanceInfoCommandHandler>();

            if (coinigyEnabled)
            {
                busBuilder.RegisterEventHandler<BalanceCheckEvent, CheckCoinigyTotalBalanceHandler>();
                busBuilder.RegisterEventHandler<BalanceCheckEvent, CheckCoinigyAccountBalancesHandler>();
                busBuilder.RegisterCommandHandler<CoinigyTotalPnLCommand, CoinigyTotalPnLHandler>();
                busBuilder.RegisterCommandHandler<CoinigyPnLForAccountCommand, CoinigyPnLForAccountHandler>();
                busBuilder.RegisterCommandHandler<SendCoinigyAccountInfoCommand, CoinigyAccountInfoHandler>();
                busBuilder.RegisterCommandHandler<GetCoinigyAccountCommand, GetCoinigyAccountHandler>();
            }

            if (poloniexEnabled)
            {
                busBuilder.RegisterEventHandler<NewTradesCheckEvent, PoloniexNewOrderCheckHandler>();
                busBuilder.RegisterEventHandler<BalanceCheckEvent, PoloniexBalanceCheckHandler>();
            }

            if (bittrexEnabled)
            {
                busBuilder.RegisterCommandHandler<BittrexTradeExportCommand, BittrexTradeExportHandler>();
                busBuilder.RegisterEventHandler<NewTradesCheckEvent, BittrexNewOrderCheckHandler>();
                busBuilder.RegisterEventHandler<BalanceCheckEvent, BittrexBalanceCheckHandler>();
                busBuilder.RegisterCommandHandler<BittrexBalanceInfoRequestedCommand, BittrexBalanceInfoRequestedHandler>();
            }

            if ((bagEnabled || dustEnabled) && bittrexEnabled)
            {
                busBuilder.RegisterEventHandler<BagAndDustEvent, BittrexBagAndDustHandler>();
            }

            if ((bagEnabled || dustEnabled) && poloniexEnabled)
            {
                busBuilder.RegisterEventHandler<BagAndDustEvent, PoloniexBagAndDustHandler>();
            }

            return busBuilder;
        }
    }
}