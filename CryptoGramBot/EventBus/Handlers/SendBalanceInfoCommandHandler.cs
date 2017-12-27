using System;
using System.Text;
using System.Threading.Tasks;
using CryptoGramBot.Configuration;
using CryptoGramBot.EventBus.Commands;
using Enexure.MicroBus;
using Microsoft.Extensions.Logging;

namespace CryptoGramBot.EventBus.Handlers
{
    public class SendBalanceInfoCommandHandler : ICommandHandler<SendBalanceInfoCommand>
    {
        private readonly IMicroBus _bus;
        private readonly GeneralConfig _generalConfig;
        private readonly ILogger<SendBalanceInfoCommandHandler> _log;

        public SendBalanceInfoCommandHandler(IMicroBus bus, ILogger<SendBalanceInfoCommandHandler> log, GeneralConfig generalConfig)
        {
            _bus = bus;
            _log = log;
            _generalConfig = generalConfig;
        }

        public async Task Handle(SendBalanceInfoCommand requestedCommand)
        {
            var sb = new StringBuilder();
            var accountName = requestedCommand.BalanceInformation.AccountName;
            var current = requestedCommand.BalanceInformation.CurrentBalance;
            var lastBalance = requestedCommand.BalanceInformation.PreviousBalance;
            var walletBalances = requestedCommand.BalanceInformation.WalletBalances;

            var timeFormat = string.Format("<strong>{0,-13}</strong>{1,-25}\n", "Time:", $"     {DateTime.Now:g}");
            var currentFormat = string.Format("<strong>{0,-13}</strong>{1,-25}\n", "Current:", $"  {current.Balance:##0.####} {_generalConfig.TradingCurrency} (${current.DollarAmount})");
            var previousFormat = string.Format("<strong>{0,-13}</strong>{1,-25}\n", "Previous:", $" {lastBalance.Balance:##0.####} {_generalConfig.TradingCurrency} (${lastBalance.DollarAmount})");
            var differenceFormat = string.Format("<strong>{0,-13}</strong>{1,-25}\n", "Difference:", $"{(current.Balance - lastBalance.Balance):##0.####} {_generalConfig.TradingCurrency} (${Math.Round(current.DollarAmount - lastBalance.DollarAmount, 2)})");

            sb.AppendLine($"<strong>24 Hour Summary</strong> for <strong>{accountName}</strong>");
            sb.AppendLine();
            sb.Append(timeFormat);
            sb.Append(currentFormat);
            sb.Append(previousFormat);
            sb.Append(differenceFormat);

            try
            {
                var percentage = Math.Round((current.Balance - lastBalance.Balance) / lastBalance.Balance * 100, 2);
                var dollarPercentage = Math.Round(
                    (current.DollarAmount - lastBalance.DollarAmount) / lastBalance.DollarAmount * 100, 2);

                var percentageFormat = string.Format("<strong>{0,-13}</strong>{1,-25}\n", "Change:", $"  {percentage}% {_generalConfig.TradingCurrency} ({dollarPercentage}% USD)");

                sb.Append(percentageFormat);
            }
            catch (Exception)
            {
                await _bus.SendAsync(new SendMessageCommand(new StringBuilder($"Could not calculate percentages. Probably because we don't have 24 hours of data yet")));
            }

            if (walletBalances != null)
            {
                sb.AppendLine("\n<strong>Wallet information</strong> (with % change since last bought)\n");

                foreach (var walletBalance in walletBalances)
                {
                    sb.AppendLine(string.Format("<strong>{0, -10}</strong> {1,-15} {2,10}", walletBalance.Currency, $"{walletBalance.BtcAmount:##0.0###} {_generalConfig.TradingCurrency}", $"{walletBalance.PercentageChange}%"));
                }
            }

            await _bus.SendAsync(new SendMessageCommand(sb));
        }
    }
}