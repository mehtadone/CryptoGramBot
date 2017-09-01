using Jojatekok.PoloniexAPI.WalletTools;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Jojatekok.PoloniexAPI
{
    public interface IWallet
    {
        /// <summary>Fetches all the balances in your account, split down into available balance, balance on orders, and the estimated BTC value of your balance.</summary>
        Task<IDictionary<string, IBalance>> GetBalancesAsync();

        /// <summary>Returns all of your deposit addresses.</summary>
        Task<IDictionary<string, string>> GetDepositAddressesAsync();

        /// <summary>Returns your deposit and withdrawal history within a range of time.</summary>
        /// <param name="startTime">The time to start fetching data from.</param>
        /// <param name="endTime">The time to stop fetching data at.</param>
        Task<IDepositWithdrawalList> GetDepositsAndWithdrawalsAsync(DateTime startTime, DateTime endTime);

        /// <summary>Returns your complete deposit and withdrawal history.</summary>
        Task<IDepositWithdrawalList> GetDepositsAndWithdrawalsAsync();

        /// <summary>Generates a new deposit address for the specified currency.</summary>
        /// <param name="currency">The ticker of the currency to generate a new address for.</param>
        Task<IGeneratedDepositAddress> PostGenerateNewDepositAddressAsync(string currency);

        /// <summary>
        ///     <para>Immediately places a withdrawal for a given currency, with no email confirmation.</para>
        ///     <para>Warning: In order to use this method, the withdrawal privilege must be enabled for your API key.</para>
        /// </summary>
        /// <param name="currency">The ticker of the currency to withdraw funds to.</param>
        /// <param name="amount">
        ///     <para>The amount of currency you wish to withdraw.</para>
        ///     <para>Note: A withdrawal fee will be applied to this amount.</para>
        /// </param>
        /// <param name="address">The address you wish to withdraw to.</param>
        /// <param name="paymentId">The payment ID you wish to use at the withdrawal.</param>
        /// <returns></returns>
        Task PostWithdrawalAsync(string currency, double amount, string address, string paymentId);

        /// <summary>
        ///     <para>Immediately places a withdrawal for a given currency, with no email confirmation.</para>
        ///     <para>Warning: In order to use this method, the withdrawal privilege must be enabled for your API key.</para>
        /// </summary>
        /// <param name="currency">The ticker of the currency to withdraw funds to.</param>
        /// <param name="amount">
        ///     <para>The amount of currency you wish to withdraw.</para>
        ///     <para>Note: A withdrawal fee will be applied to this amount.</para>
        /// </param>
        /// <param name="address">The address you wish to withdraw to.</param>
        Task PostWithdrawalAsync(string currency, double amount, string address);
    }
}
