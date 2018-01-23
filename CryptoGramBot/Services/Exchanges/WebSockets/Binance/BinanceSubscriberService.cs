using Binance.Api;
using Binance.Api.WebSocket;
using Binance.Api.WebSocket.Events;
using CryptoGramBot.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoGramBot.Services.Exchanges.WebSockets.Binance
{
    public class BinanceSubscriberService : IBinanceSubscriberService
    {
        #region Consts

        /// <summary>
        /// Binance websocket connection life time.
        /// "A single connection to stream.binance.com is only valid for 24 hours; expect to be disconnected at the 24 hour mark"        
        /// on 10 minutes less than 24 hours, to prevent Binance disconnect
        /// <see cref="https://github.com/binance-exchange/binance-official-api-docs/blob/master/web-socket-streams.md"/>
        /// </summary>
        private readonly int WEBSOCKET_LIFE_TIME_IN_MINUTES = 1430;

        #endregion

        #region Fields

        private bool _isDisposed;

        private CancellationTokenSource _userDataCancellationTokenSource;
        private CancellationTokenSource _symbolStatisticCancellationTokenSource;

        private ISymbolStatisticsWebSocketClient _symbolStatisticsWebSocketClient;
        private IUserDataWebSocketClient _userDataWebSocketClient;

        private Timer _symbolsReConnectionTimer;
        private Timer _userDataReConnectionTimer;

        private Task _userDataSubscribeTask;
        private Task _symbolsSubscribeTask;

        private Action<OrderUpdateEventArgs> _onOrderUpdate;
        private Action<AccountUpdateEventArgs> _onAccountUpdate;
        private Action<AccountTradeUpdateEventArgs> _onAccountTradeUpdate;
        private Action<SymbolStatisticsEventArgs> _onSymbolStatisticUpdate;

        private BinanceApiUser _user;

        #endregion

        #region Dependecies

        private readonly IServiceProvider _serviceProvider;
        private readonly BinanceConfig _config;

        #endregion

        #region Constructor

        public BinanceSubscriberService(BinanceConfig config,
            IServiceProvider serviceProvider)
        {
            _config = config;
            _serviceProvider = serviceProvider;
        }

        #endregion

        #region IBinanceSubscribersService

        public async Task SymbolsStatistics(Action<SymbolStatisticsEventArgs> onUpdate)
        {
            if (_symbolsSubscribeTask == null)
            {
                _onSymbolStatisticUpdate = onUpdate ?? throw new ArgumentException(nameof(onUpdate));

                await SubscribeToSymbols();
            }
        }

        public async Task UserData(Action<OrderUpdateEventArgs> onOrderUpdate, Action<AccountUpdateEventArgs> onAccountUpdate, Action<AccountTradeUpdateEventArgs> onAccountTradeUpdate)
        {
            if (_userDataSubscribeTask == null)
            {
                _onOrderUpdate = onOrderUpdate ?? throw new ArgumentException(nameof(onOrderUpdate));
                _onAccountUpdate = onAccountUpdate ?? throw new ArgumentException(nameof(onAccountUpdate));
                _onAccountTradeUpdate = onAccountTradeUpdate ?? throw new ArgumentException(nameof(onAccountTradeUpdate));

                await SubscribeToUserData();
            }
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            if (!_isDisposed)
            {

                _symbolsReConnectionTimer?.Dispose();
                _userDataReConnectionTimer?.Dispose();

                _symbolStatisticCancellationTokenSource?.Cancel();
                _userDataCancellationTokenSource?.Cancel();

                _symbolStatisticCancellationTokenSource?.Dispose();
                _userDataCancellationTokenSource?.Dispose();

                _isDisposed = true;
            }
        }

        #endregion

        #region Private methods

        private async Task SubscribeToSymbols(bool reConnect = false)
        {
            if (_symbolsSubscribeTask != null && !reConnect)
            {
                return;
            }

            try
            {
                _symbolStatisticCancellationTokenSource?.Cancel();
                _symbolStatisticCancellationTokenSource?.Dispose();

                _symbolStatisticCancellationTokenSource = new CancellationTokenSource();

                _symbolStatisticsWebSocketClient = _serviceProvider.GetService<ISymbolStatisticsWebSocketClient>();

                _symbolsSubscribeTask = _symbolStatisticsWebSocketClient.SubscribeAsync(_onSymbolStatisticUpdate, _symbolStatisticCancellationTokenSource.Token);

                if (!reConnect)
                {
                    SymbolsReConnectionTimerInitialize();
                }

                await _symbolsSubscribeTask;
            }
            catch (Exception)
            {
                _symbolsSubscribeTask = null;
            }
        }

        private async Task SubscribeToUserData(bool reConnect = false)
        {
            if (_userDataSubscribeTask != null && !reConnect)
            {
                return;
            }

            try
            {
                _user?.Dispose();

                _userDataCancellationTokenSource?.Cancel();
                _userDataCancellationTokenSource?.Dispose();

                _userDataCancellationTokenSource = new CancellationTokenSource();

                if(_userDataWebSocketClient != null)
                {
                    _userDataWebSocketClient.TradeUpdate -= OnAccountTradeUpdate;
                    _userDataWebSocketClient.AccountUpdate -= OnAccountUpdate;
                    _userDataWebSocketClient.OrderUpdate -= OnOrderUpdate;
                }

                _userDataWebSocketClient = _serviceProvider.GetService<IUserDataWebSocketClient>();

                _user = new BinanceApiUser(_config.Key, _config.Secret);

                _userDataWebSocketClient.TradeUpdate += OnAccountTradeUpdate;
                _userDataWebSocketClient.AccountUpdate += OnAccountUpdate;
                _userDataWebSocketClient.OrderUpdate += OnOrderUpdate;

                _userDataSubscribeTask = _userDataWebSocketClient.SubscribeAsync(_user, _userDataCancellationTokenSource.Token);

                if (!reConnect)
                {
                    UserDataReConnectionTimerInitialize();
                }

                await _userDataSubscribeTask;
            }
            catch (Exception)
            {
                _userDataSubscribeTask = null;
            }
        }

        private void OnAccountTradeUpdate(object sender, AccountTradeUpdateEventArgs args)
        {
            _onAccountTradeUpdate?.Invoke(args);
        }

        private void OnAccountUpdate(object sender, AccountUpdateEventArgs args)
        {
            _onAccountUpdate?.Invoke(args);
        }

        private void OnOrderUpdate(object sender, OrderUpdateEventArgs args)
        {
            _onOrderUpdate?.Invoke(args);
        }

        private void SymbolsReConnectionTimerInitialize()
        {
            _symbolsReConnectionTimer?.Dispose();

            _symbolsReConnectionTimer = new Timer(s => SubscribeToSymbols(reConnect: true), null,
                TimeSpan.FromMinutes(WEBSOCKET_LIFE_TIME_IN_MINUTES),
                TimeSpan.FromMinutes(WEBSOCKET_LIFE_TIME_IN_MINUTES));
        }

        private void UserDataReConnectionTimerInitialize()
        {
            _userDataReConnectionTimer?.Dispose();

            _userDataReConnectionTimer = new Timer(s => SubscribeToUserData(reConnect: true), null,
                TimeSpan.FromMinutes(WEBSOCKET_LIFE_TIME_IN_MINUTES),
                TimeSpan.FromMinutes(WEBSOCKET_LIFE_TIME_IN_MINUTES));
        } 

        #endregion
    }
}
