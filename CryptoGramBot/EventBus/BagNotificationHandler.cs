using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CryptoGramBot.EventBus;
using CryptoGramBot.Models;
using Enexure.MicroBus;

namespace CryptoGramBot.Services
{
    public class BagNotificationHandler : ICommandHandler<SendBagNotificationCommand>
    {
        private readonly IMicroBus _bus;

        public BagNotificationHandler(IMicroBus bus)
        {
            _bus = bus;
        }

        public async Task Handle(SendBagNotificationCommand command)
        {
            var message =
                $"{DateTime.Now:g}\n" +
                $"<strong>Bag detected for {command.WalletBalance.Currency}</strong>\n" +
                $"Bought price: {command.LastTradeForPair.Limit:#0.#############}\n" +
                $"Current price: {command.CurrentPrice:#0.#############}\n" +
                $"Percentage drop: {command.Percentage}%\n" +
                $"Bought on: {command.LastTradeForPair.TimeStamp:g}\n" +
                $"Value: {(command.WalletBalance.Balance * command.CurrentPrice):#0.#############}";

            await _bus.SendAsync(new SendMessageCommand(message));
        }
    }

    public class SendBagNotificationCommand : ICommand
    {
        public SendBagNotificationCommand(WalletBalance walletBalance, Trade lastTradeForPair, decimal currentPrice, decimal percentage)
        {
            WalletBalance = walletBalance;
            LastTradeForPair = lastTradeForPair;
            CurrentPrice = currentPrice;
            Percentage = percentage;
        }

        public decimal CurrentPrice { get; }
        public Trade LastTradeForPair { get; }
        public decimal Percentage { get; }
        public WalletBalance WalletBalance { get; }
    }
}