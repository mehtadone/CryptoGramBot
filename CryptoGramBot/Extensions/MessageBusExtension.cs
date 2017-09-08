using CryptoGramBot.Configuration;
using CryptoGramBot.EventBus;
using CryptoGramBot.EventBus.Events;
using CryptoGramBot.EventBus.Handlers;
using CryptoGramBot.EventBus.Queries;
using CryptoGramBot.Services;
using Enexure.MicroBus;

namespace CryptoGramBot.Extensions
{
    public static class MessageBusExtension
    {
        public static BusBuilder ConfigureCore(this BusBuilder busBuilder, bool coinigyEnabled, bool bittrexEnabled, bool poloniexEnabled, bool bagEnabled)
        {
            busBuilder.RegisterCommandHandler<SendMessageCommand, SendMessageHandler>();
            busBuilder.RegisterCommandHandler<SendFileCommand, SendFileHandler>();
            busBuilder.RegisterCommandHandler<SendHelpMessageCommand, HelpMessageHandler>();
            busBuilder.RegisterCommandHandler<PairProfitCommand, PairProfitHandler>();
            busBuilder.RegisterCommandHandler<ExcelExportCommand, ExcelExportHandler>();
            busBuilder.RegisterCommandHandler<TradeNotificationCommand, TradeNotificationHandler>();
            busBuilder.RegisterQueryHandler<LastCheckedQuery, LastCheckedResponse, LastCheckedHandler>();
            busBuilder.RegisterQueryHandler<FindNewTradeQuery, FindNewTradesResponse, SaveAndFindNewTradesHandler>();
            busBuilder.RegisterCommandHandler<AddLastCheckedCommand, AddLastCheckedHandler>();

            if (coinigyEnabled)
            {
                busBuilder.RegisterCommandHandler<CheckCoinigyTotalBalanceCommand, CheckCoinigyTotalBalanceHandler>();
                busBuilder.RegisterCommandHandler<CheckCoinigyAccountBalancesCommand, CheckCoinigyAccountBalancesHandler>();
                busBuilder.RegisterCommandHandler<CoinigyTotalPnLCommand, CoinigyTotalPnLHandler>();
                busBuilder.RegisterCommandHandler<CoinigyPnLForAccountCommand, CoinigyPnLForAccountHandler>();
                busBuilder.RegisterCommandHandler<CoinigyBalanceUpdateCommand, CoinigyBalanceUpdateHandler>();
                busBuilder.RegisterCommandHandler<SendCoinigyAccountInfoCommand, CoinigyAccountInfoHandler>();
            }

            if (poloniexEnabled)
            {
                busBuilder.RegisterEventHandler<NewTradesCheckEvent, PoloniexNewOrderCheckHandler>();
            }

            if (bittrexEnabled)
            {
                busBuilder.RegisterCommandHandler<BittrexTradeExportCommand, BittrexTradeExportHandler>();
                busBuilder.RegisterEventHandler<NewTradesCheckEvent, BittrexNewOrderCheckHandler>();
            }

            if (bagEnabled)
            {
                busBuilder.RegisterCommandHandler<BagManagementCommand, BagManagementHandler>();
            }

            return busBuilder;
        }
    }
}