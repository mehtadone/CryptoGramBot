using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Binance.Api;
using Binance.Api.WebSocket;
using Binance.Api.WebSocket.Events;
using CryptoGramBot.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CryptoGramBot.Services.Exchanges.WebSockets.Binance
{
    public class BinanceSubscribersService : IBinanceSubscribersService
    {
        #region Consts

        private readonly int WEBSOCKET_LIFE_TIME_IN_MINUTES = 1430; //for 10 minutes less than 24 hours, in order not to wait for the connection to break from Binance

        #endregion

        #region Fields

        private bool _isDisposed;

        private CancellationTokenSource _userDataCancellationTokenSource;
        private CancellationTokenSource _symbolStatisticCancellationTokenSource;

        private ISymbolStatisticsWebSocketClient _symbolStatisticsWebSocketClient;
        private ICustomUserDataWebSocketClient _userDataWebSocketClient;

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

        public BinanceSubscribersService(BinanceConfig config,
            IServiceProvider serviceProvider)
        {
            _config = config;
            _serviceProvider = serviceProvider;
        }

        #endregion

        #region IBinanceSubscribersService

        public void AddSymbols(Action<SymbolStatisticsEventArgs> onUpdate)
        {
            if (_symbolsSubscribeTask == null)
            {
                _onSymbolStatisticUpdate = onUpdate ?? throw new ArgumentException(nameof(onUpdate));

                SubscribeSymbols();
            }
        }

        public void AddUserData(Action<OrderUpdateEventArgs> onOrderUpdate, Action<AccountUpdateEventArgs> onAccountUpdate, Action<AccountTradeUpdateEventArgs> onAccountTradeUpdate)
        {
            if (_userDataSubscribeTask == null)
            {
                _onOrderUpdate = onOrderUpdate ?? throw new ArgumentException(nameof(onOrderUpdate));
                _onAccountUpdate = onAccountUpdate ?? throw new ArgumentException(nameof(onAccountUpdate)); ;
                _onAccountTradeUpdate = onAccountTradeUpdate ?? throw new ArgumentException(nameof(onAccountTradeUpdate)); ;

                SubscribeUserData();
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
                _symbolStatisticCancellationTokenSource?.Dispose();
                _userDataCancellationTokenSource?.Dispose();

                _isDisposed = true;
            }
        }

        #endregion

        #region Private methods

        private void SubscribeSymbols(bool reConnect = false)
        {
            if (_symbolsSubscribeTask != null && !reConnect)
            {
                return;
            }

            try
            {
                _symbolStatisticCancellationTokenSource?.Dispose();

                _symbolStatisticCancellationTokenSource = new CancellationTokenSource();

                _symbolStatisticsWebSocketClient = _serviceProvider.GetService<ISymbolStatisticsWebSocketClient>();

                _symbolsSubscribeTask = _symbolStatisticsWebSocketClient.SubscribeAsync(_onSymbolStatisticUpdate, _symbolStatisticCancellationTokenSource.Token);

                SymbolsReConnectionTimerInitialize();
            }
            catch (Exception)
            {
                _symbolsSubscribeTask = null;
            }
        }

        private void SubscribeUserData(bool reConnect = false)
        {
            if (_userDataSubscribeTask != null && !reConnect)
            {
                return;
            }

            try
            {
                _user?.Dispose();

                _userDataCancellationTokenSource?.Dispose();

                _userDataCancellationTokenSource = new CancellationTokenSource();

                _userDataWebSocketClient = _serviceProvider.GetService<ICustomUserDataWebSocketClient>();

                _userDataWebSocketClient.TradeUpdate += (o, a) => _onAccountTradeUpdate(a);
                _userDataWebSocketClient.AccountUpdate += (o, a) => _onAccountUpdate(a);
                _userDataWebSocketClient.OrderUpdate += (o, a) => _onOrderUpdate(a);

                _userDataSubscribeTask = _userDataWebSocketClient.SubscribeAsync(_user, _userDataCancellationTokenSource.Token);

                UserDataReConnectionTimerInitialize();
            }
            catch (Exception)
            {
                _userDataSubscribeTask = null;
            }
        }

        private void SymbolsReConnectionTimerInitialize()
        {
            _symbolsReConnectionTimer?.Dispose();

            _symbolsReConnectionTimer = new Timer(s => SubscribeSymbols(reConnect: true), null,
                TimeSpan.FromMinutes(WEBSOCKET_LIFE_TIME_IN_MINUTES),
                TimeSpan.FromMinutes(WEBSOCKET_LIFE_TIME_IN_MINUTES));
        }

        private void UserDataReConnectionTimerInitialize()
        {
            _userDataReConnectionTimer?.Dispose();

            _userDataReConnectionTimer = new Timer(s => SubscribeUserData(reConnect: true), null,
                TimeSpan.FromMinutes(WEBSOCKET_LIFE_TIME_IN_MINUTES),
                TimeSpan.FromMinutes(WEBSOCKET_LIFE_TIME_IN_MINUTES));
        } 

        #endregion
    }
}
