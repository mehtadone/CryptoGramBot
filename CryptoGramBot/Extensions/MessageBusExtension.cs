using CryptoGramBot.EventBus;
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
            busBuilder.RegisterCommandHandler<SendBagNotificationCommand, BagNotificationHandler>();
            busBuilder.RegisterCommandHandler<SendCoinigyAccountInfoCommand, CoinigyAccountInfoHandler>();
            busBuilder.RegisterCommandHandler<PairProfitCommand, PairProfitHandler>();
            busBuilder.RegisterCommandHandler<BalanceUpdateCommand, BalanceUpdateHandler>();
            busBuilder.RegisterCommandHandler<ExcelExportCommand, ExcelExportHandler>();
            busBuilder.RegisterCommandHandler<PnLForAccountCommand, PnLForAccountHandler>();
            busBuilder.RegisterCommandHandler<TradeNotificationCommand, TradeNotificationHandler>();
            busBuilder.RegisterCommandHandler<TotalPnLCommand, TotalPnLHandler>();
            busBuilder.RegisterCommandHandler<BittrexTradeExportCommand, BittrexTradeExportHandler>();
            return busBuilder;
        }
    }
}