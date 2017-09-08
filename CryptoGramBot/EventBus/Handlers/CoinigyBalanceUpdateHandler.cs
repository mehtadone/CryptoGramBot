using System;
using System.Threading.Tasks;
using CryptoGramBot.Models;
using Enexure.MicroBus;

namespace CryptoGramBot.EventBus.Handlers
{
    public class CoinigyBalanceUpdateCommand : ICommand
    {
        public CoinigyBalanceUpdateCommand(BalanceHistory current, BalanceHistory lastBalance, string accountName)
        {
            Current = current;
            LastBalance = lastBalance;
            AccountName = accountName;
        }

        public string AccountName { get; }
        public BalanceHistory Current { get; }
        public BalanceHistory LastBalance { get; }
    }

    public class CoinigyBalanceUpdateHandler : ICommandHandler<CoinigyBalanceUpdateCommand>
    {
        private readonly IMicroBus _bus;

        public CoinigyBalanceUpdateHandler(IMicroBus bus)
        {
            _bus = bus;
        }

        public async Task Handle(CoinigyBalanceUpdateCommand command)
        {
            var accountName = command.AccountName;
            var current = command.Current;
            var lastBalance = command.LastBalance;

            var message = $"<strong>24 Hour Summary</strong> for <strong>{accountName}</strong>\n\n" +
                          $"{DateTime.Now:g}\n" +
                          $"<strong>Current</strong>: {current.Balance} BTC (${current.DollarAmount})\n" +
                          $"<strong>Previous</strong>: {lastBalance.Balance} BTC (${lastBalance.DollarAmount})\n" +
                          $"<strong>Difference</strong>: {(current.Balance - lastBalance.Balance):##0.###########} BTC (${Math.Round(current.DollarAmount - lastBalance.DollarAmount, 2)})\n";

            try
            {
                var percentage = Math.Round((current.Balance - lastBalance.Balance) / lastBalance.Balance * 100, 2);

                var dollarPercentage = Math.Round(
                    (current.DollarAmount - lastBalance.DollarAmount) / lastBalance.DollarAmount * 100, 2);

                message = message + $"<strong>Change</strong>: {percentage}% BTC ({dollarPercentage}% USD)";
            }
            catch (Exception ex)
            {
                await _bus.SendAsync(new SendMessageCommand($"Could not calculate percentages - { ex.Message }"));
            }
            await _bus.SendAsync(new SendMessageCommand(message));
        }
    }
}