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
        public static BusBuilder ConfigureCore(this BusBuilder busBuilder)
        {
            busBuilder.RegisterCommandHandler<SendMessageCommand, SendMessageHandler>();
            busBuilder.RegisterCommandHandler<SendFileCommand, SendFileHandler>();
            busBuilder.RegisterCommandHandler<SendHelpMessageCommand, HelpMessageHandler>();
            busBuilder.RegisterCommandHandler<BagManagementCommand, BagManagementHandler>();
            busBuilder.RegisterCommandHandler<SendCoinigyAccountInfoCommand, CoinigyAccountInfoHandler>();
            busBuilder.RegisterCommandHandler<PairProfitCommand, PairProfitHandler>();
            busBuilder.RegisterCommandHandler<BalanceUpdateCommand, BalanceUpdateHandler>();
            busBuilder.RegisterCommandHandler<ExcelExportCommand, ExcelExportHandler>();
            busBuilder.RegisterCommandHandler<PnLForAccountCommand, PnLForAccountHandler>();
            busBuilder.RegisterCommandHandler<TradeNotificationCommand, TradeNotificationHandler>();
            busBuilder.RegisterCommandHandler<TotalPnLCommand, TotalPnLHandler>();
            busBuilder.RegisterCommandHandler<BittrexTradeExportCommand, BittrexTradeExportHandler>();
            busBuilder.RegisterQueryHandler<LastCheckedQuery, LastCheckedResponse, LastCheckedHandler>();
            busBuilder.RegisterQueryHandler<FindNewTradeQuery, FindNewTradesResponse, SaveAndFindNewTradesHandler>();
            busBuilder.RegisterEventHandler<NewTradesCheckEvent, BittrexNewOrderCheckHandler>();
            busBuilder.RegisterEventHandler<NewTradesCheckEvent, PoloniexNewOrderCheckHandler>();
            busBuilder.RegisterCommandHandler<AddLastCheckedCommand, AddLastCheckedHandler>();
            return busBuilder;
        }
    }
}