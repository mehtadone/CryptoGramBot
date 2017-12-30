using System;
using System.Threading.Tasks;
using CryptoGramBot.Configuration;
using CryptoGramBot.EventBus.Commands;
using CryptoGramBot.Helpers;
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
            var sb = new StringBuffer();
            var accountName = requestedCommand.BalanceInformation.AccountName;
            var current = requestedCommand.BalanceInformation.CurrentBalance;
            var lastBalance = requestedCommand.BalanceInformation.PreviousBalance;
            var walletBalances = requestedCommand.BalanceInformation.WalletBalances;

            var timeFormat = string.Format("{2}{0,-13}{3}{1,-25}\n", "Time:", $"     {DateTime.Now:g}", StringContants.StrongOpen, StringContants.StrongClose);
            var currentFormat = string.Format("{2}{0,-13}{3}{1,-25}\n", "Current:", $"  {current.Balance:##0.####} {_generalConfig.TradingCurrency} (${current.DollarAmount})", StringContants.StrongOpen, StringContants.StrongClose);
            var previousFormat = string.Format("{2}{0,-13}{3}{1,-25}\n", "Previous:", $" {lastBalance.Balance:##0.####} {_generalConfig.TradingCurrency} (${lastBalance.DollarAmount})", StringContants.StrongOpen, StringContants.StrongClose);
            var differenceFormat = string.Format("{2}{0,-13}{3}{1,-25}\n", "Difference:", $"{(current.Balance - lastBalance.Balance):##0.####} {_generalConfig.TradingCurrency} (${Math.Round(current.DollarAmount - lastBalance.DollarAmount, 2)})", StringContants.StrongOpen, StringContants.StrongClose);

            sb.Append(string.Format("{1}24 Hour Summary{2} for {1}{0}{2}\n\n", accountName, StringContants.StrongOpen, StringContants.StrongClose));
            sb.Append(timeFormat);
            sb.Append(currentFormat);
            sb.Append(previousFormat);
            sb.Append(differenceFormat);

            try
            {
                var percentage = Math.Round((current.Balance - lastBalance.Balance) / lastBalance.Balance * 100, 2);
                var dollarPercentage = Math.Round(
                    (current.DollarAmount - lastBalance.DollarAmount) / lastBalance.DollarAmount * 100, 2);

                var percentageFormat = string.Format("{2}{0,-13}{3}{1,-25}\n", "Change:", $"  {percentage}% {_generalConfig.TradingCurrency} ({dollarPercentage}% USD)", StringContants.StrongOpen, StringContants.StrongClose);

                sb.Append(percentageFormat);
            }
            catch (Exception)
            {
                var message = new StringBuffer();
                message.Append(StringContants.No24HourOfData);
                await _bus.SendAsync(new SendMessageCommand(message));
            }

            if (walletBalances != null)
            {
                sb.Append($"\n{StringContants.StrongOpen}Wallet information{StringContants.StrongClose} (with % change since last bought)\n\n");

                foreach (var walletBalance in walletBalances)
                {
                    if (walletBalance.BtcAmount >= _generalConfig.IgnoreDustInTradingCurrency)
                        sb.Append(string.Format("{3}{0,-10}{4} {1,-15} {2,10}", walletBalance.Currency, $"{walletBalance.BtcAmount:##0.0###} {_generalConfig.TradingCurrency}", $"{walletBalance.PercentageChange}%\n", StringContants.StrongOpen, StringContants.StrongClose));
                }
            }

            await _bus.SendAsync(new SendMessageCommand(sb));
        }
    }
}