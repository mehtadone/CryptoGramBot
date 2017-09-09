using System;
using System.Threading.Tasks;
using CryptoGramBot.EventBus.Commands;
using Enexure.MicroBus;
using Microsoft.Extensions.Logging;
using Serilog;

namespace CryptoGramBot.EventBus.Handlers.BalanceInfo
{
    public class SendBalanceInfoCommandHandler : ICommandHandler<SendBalanceInfoCommand>
    {
        private readonly IMicroBus _bus;
        private readonly ILogger<SendBalanceInfoCommandHandler> _log;

        public SendBalanceInfoCommandHandler(IMicroBus bus, ILogger<SendBalanceInfoCommandHandler> log)
        {
            _bus = bus;
            _log = log;
        }

        public async Task Handle(SendBalanceInfoCommand requestedCommand)
        {
            var accountName = requestedCommand.AccountName;
            var current = requestedCommand.Current;
            var lastBalance = requestedCommand.LastBalance;
            var walletBalances = requestedCommand.WalletBalances;

            var timeFormat = string.Format("<strong>{0,-13}</strong>{1,-25}\n", "Time:", $"     {DateTime.Now:g}");
            var currentFormat = string.Format("<strong>{0,-13}</strong>{1,-25}\n", "Current:", $"  {current.Balance:##0.####} BTC (${current.DollarAmount})");
            var previousFormat = string.Format("<strong>{0,-13}</strong>{1,-25}\n", "Previous:", $" {lastBalance.Balance:##0.####} BTC (${lastBalance.DollarAmount})");
            var differenceFormat = string.Format("<strong>{0,-13}</strong>{1,-25}\n", "Difference:", $"{(current.Balance - lastBalance.Balance):##0.####} BTC (${Math.Round(current.DollarAmount - lastBalance.DollarAmount, 2)})");

            var message = $"<strong>24 Hour Summary</strong> for <strong>{accountName}</strong>\n\n" +
                          timeFormat + currentFormat + previousFormat + differenceFormat;

            try
            {
                var percentage = Math.Round((current.Balance - lastBalance.Balance) / lastBalance.Balance * 100, 2);
                var dollarPercentage = Math.Round(
                    (current.DollarAmount - lastBalance.DollarAmount) / lastBalance.DollarAmount * 100, 2);

                var percentageFormat = string.Format("<strong>{0,-13}</strong>{1,-25}\n", "Change:", $"  {percentage}% BTC ({dollarPercentage}% USD)");

                message = message + percentageFormat;
            }
            catch (Exception ex)
            {
                await _bus.SendAsync(new SendMessageCommand($"Could not calculate percentages. Probably because we don't have 24 hours of data yet"));
            }

            if (walletBalances != null)
            {
                message = message + "\n<strong>Wallet information</strong>\n\n";

                foreach (var walletBalance in walletBalances)
                {
                    message =
                        message + string.Format("<strong>{0, -20}</strong> {1}\n", walletBalance.Currency, $"{walletBalance.BtcAmount:##0.0000} BTC");
                }
            }

            await _bus.SendAsync(new SendMessageCommand(message));
        }
    }
}