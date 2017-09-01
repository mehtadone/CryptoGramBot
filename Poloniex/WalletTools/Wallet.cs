using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Jojatekok.PoloniexAPI.WalletTools
{
    public class Wallet : IWallet
    {
        private ApiWebClient ApiWebClient { get; set; }

        internal Wallet(ApiWebClient apiWebClient)
        {
            ApiWebClient = apiWebClient;
        }

        private IDictionary<string, IBalance> GetBalances()
        {
            var postData = new Dictionary<string, object>();

            var data = PostData<IDictionary<string, IBalance>>("returnCompleteBalances", postData);
            return data;
        }

        private IDictionary<string, string> GetDepositAddresses()
        {
            var postData = new Dictionary<string, object>();

            var data = PostData<IDictionary<string, string>>("returnDepositAddresses", postData);
            return data;
        }

        private IDepositWithdrawalList GetDepositsAndWithdrawals(DateTime startTime, DateTime endTime)
        {
            var postData = new Dictionary<string, object> {
                { "start", Helper.DateTimeToUnixTimeStamp(startTime) },
                { "end", Helper.DateTimeToUnixTimeStamp(endTime) }
            };

            var data = PostData<DepositWithdrawalList>("returnDepositsWithdrawals", postData);
            return data;
        }

        private IGeneratedDepositAddress PostGenerateNewDepositAddress(string currency)
        {
            var postData = new Dictionary<string, object> {
                { "currency", currency }
            };

            var data = PostData<IGeneratedDepositAddress>("generateNewAddress", postData);
            return data;
        }

        private void PostWithdrawal(string currency, double amount, string address, string paymentId)
        {
            var postData = new Dictionary<string, object> {
                { "currency", currency },
                { "amount", amount.ToStringNormalized() },
                { "address", address }
            };

            if (paymentId != null) {
                postData.Add("paymentId", paymentId);
            }

            PostData<IGeneratedDepositAddress>("withdraw", postData);
        }

        public Task<IDictionary<string, IBalance>> GetBalancesAsync()
        {
            return Task.Factory.StartNew(() => GetBalances());
        }

        public Task<IDictionary<string, string>> GetDepositAddressesAsync()
        {
            return Task.Factory.StartNew(() => GetDepositAddresses());
        }

        public Task<IDepositWithdrawalList> GetDepositsAndWithdrawalsAsync(DateTime startTime, DateTime endTime)
        {
            return Task.Factory.StartNew(() => GetDepositsAndWithdrawals(startTime, endTime));
        }

        public Task<IDepositWithdrawalList> GetDepositsAndWithdrawalsAsync()
        {
            return Task.Factory.StartNew(() => GetDepositsAndWithdrawals(Helper.DateTimeUnixEpochStart, DateTime.MaxValue));
        }

        public Task<IGeneratedDepositAddress> PostGenerateNewDepositAddressAsync(string currency)
        {
            return Task.Factory.StartNew(() => PostGenerateNewDepositAddress(currency));
        }

        public Task PostWithdrawalAsync(string currency, double amount, string address, string paymentId)
        {
            return Task.Factory.StartNew(() => PostWithdrawal(currency, amount, address, paymentId));
        }

        public Task PostWithdrawalAsync(string currency, double amount, string address)
        {
            return Task.Factory.StartNew(() => PostWithdrawal(currency, amount, address, null));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private T PostData<T>(string command, Dictionary<string, object> postData)
        {
            return ApiWebClient.PostData<T>(command, postData);
        }
    }
}
