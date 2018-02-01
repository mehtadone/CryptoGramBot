using Binance.Api;
using Binance.Api.WebSocket;
using Binance.Api.WebSocket.Events;
using Binance.Market;
using CryptoGramBot.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoGramBot.Services.Exchanges.WebSockets.Binance
{
    public class BinanceSubscriberService : IBinanceSubscriberService
    {
        #region Private Classes

        private class CandlestickSubscriber
        {
            public string Symbol { get; set; }

            public CandlestickInterval Interval { get; set; }

            public Timer CandlestickReConnectionTimer { get; set; }

            public CancellationTokenSource TokenSource { get; set; }

            public ICandlestickWebSocketClient CandlestickWebSocketClient { get; set; }

            public Task SubscribeTask { get; set; }
        }

        #endregion

        #region Consts

        /// <summary>
        /// Binance websocket connection life time.
        /// "A single connection to stream.binance.com is only valid for 24 hours; expect to be disconnected at the 24 hour mark"        
        /// on 60 minutes less than 24 hours, to prevent Binance disconnect
        /// <see cref="https://github.com/binance-exchange/binance-official-api-docs/blob/master/web-socket-streams.md"/>
        /// </summary>
        private readonly int WEBSOCKET_LIFE_TIME_IN_MINUTES = 1380;

        #endregion

        #region Fields

        private bool _isDisposed;

        private CancellationTokenSource _userDataCancellationTokenSource;
        private CancellationTokenSource _symbolStatisticCancellationTokenSource;

        private ConcurrentDictionary<string, CandlestickSubscriber> _candlestickSubscribers;

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

        private Action<CandlestickEventArgs> _onCandlestickUpdate;

        private Action _onSymbolStatisticError;
        private Action<string, CandlestickInterval> _onCandlestickError;
        private Func<Task> _onUserDataError;

        private BinanceApiUser _user;

        #endregion

        #region Dependecies

        private readonly IServiceProvider _serviceProvider;
        private readonly BinanceConfig _config;
        private readonly ILogger<BinanceSubscriberService> _log;

        #endregion

        #region Constructor

        public BinanceSubscriberService(BinanceConfig config,
            IServiceProvider serviceProvider,
            ILogger<BinanceSubscriberService> log)
        {
            _config = config;
            _serviceProvider = serviceProvider;
            _log = log;
        }

        #endregion

        #region IBinanceSubscribersService

        public async Task SymbolsStatistics(Action<SymbolStatisticsEventArgs> onUpdate, Action onError)
        {
            if (_symbolsSubscribeTask == null)
            {
                _onSymbolStatisticUpdate = onUpdate ?? throw new ArgumentException(nameof(onUpdate));
                _onSymbolStatisticError = onError ?? throw new ArgumentException(nameof(onError));                

                await SubscribeToSymbols();
            }
        }

        public async Task UserData(Action<OrderUpdateEventArgs> onOrderUpdate, 
            Action<AccountUpdateEventArgs> onAccountUpdate, 
            Action<AccountTradeUpdateEventArgs> onAccountTradeUpdate,
            Func<Task> onError)
        {
            if (_userDataSubscribeTask == null)
            {
                _onOrderUpdate = onOrderUpdate ?? throw new ArgumentException(nameof(onOrderUpdate));
                _onAccountUpdate = onAccountUpdate ?? throw new ArgumentException(nameof(onAccountUpdate));
                _onAccountTradeUpdate = onAccountTradeUpdate ?? throw new ArgumentException(nameof(onAccountTradeUpdate));
                _onUserDataError = onError ?? throw new ArgumentException(nameof(onError));

                _log.LogInformation($"Subscribe user data");

                await SubscribeToUserData();
            }
        }

        public async Task Candlestick(string symbol, CandlestickInterval interval,
            Action<CandlestickEventArgs> onUpdate,
            Action<string, CandlestickInterval> onError)
        {
            if (_candlestickSubscribers == null || !_candlestickSubscribers.ContainsKey($"{symbol}{interval.AsString()}"))
            {
                _onCandlestickUpdate = onUpdate ?? throw new ArgumentException(nameof(onUpdate));
                _onCandlestickError = onError ?? throw new ArgumentException(nameof(onError));

                if (_candlestickSubscribers == null)
                {
                    _candlestickSubscribers = new ConcurrentDictionary<string, CandlestickSubscriber>();
                }

                await SubscribeToCandlestick(symbol, interval);
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

                if(_candlestickSubscribers != null)
                {
                    foreach (var item in _candlestickSubscribers)
                    {
                        item.Value.CandlestickReConnectionTimer?.Dispose();
                        item.Value.TokenSource?.Cancel();
                        item.Value.TokenSource?.Dispose();
                    }
                }              

                _isDisposed = true;
            }
        }

        #endregion

        #region Private methods

        private async Task SubscribeToSymbols(bool reConnect = false)
        {
            _log.LogInformation($"Subscribe to symbols");

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

                _log.LogError($"Error with symbols statistic websocket. Cache will be clear");

                _onSymbolStatisticError?.Invoke();
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

                _log.LogError($"Error with user data websocket. Cache will be clear");

                await _onUserDataError?.Invoke();
            }
        }

        private async Task SubscribeToCandlestick(string symbol, CandlestickInterval interval, bool reConnect = false)
        {
            _log.LogInformation($"Subscribe to candlestick {symbol} {interval.AsString()}");

            var key = GetKey(symbol, interval);

            if (_candlestickSubscribers.ContainsKey(key) && !reConnect)
            {
                return;
            }

            try
            {
                CandlestickSubscriber subscriber = _candlestickSubscribers.ContainsKey(key) ? _candlestickSubscribers[key] : null;

                if(subscriber != null)
                {
                    subscriber.TokenSource.Cancel();
                    subscriber.TokenSource.Dispose();
                }
                else
                {
                    subscriber = new CandlestickSubscriber()
                    {
                        Symbol = symbol,
                        Interval = interval
                    };

                    CandlestickReConnectionTimerInitialize(subscriber);

                    _candlestickSubscribers[key] = subscriber;
                }

                subscriber.TokenSource = new CancellationTokenSource();
                subscriber.CandlestickWebSocketClient = _serviceProvider.GetService<ICandlestickWebSocketClient>();
                subscriber.SubscribeTask = subscriber.CandlestickWebSocketClient.SubscribeAsync(symbol, interval, _onCandlestickUpdate, subscriber.TokenSource.Token);

                await subscriber.SubscribeTask;
            }
            catch (Exception)
            {
                if (_candlestickSubscribers.ContainsKey(key))
                {
                    var subscriber = _candlestickSubscribers[key];

                    subscriber.CandlestickReConnectionTimer?.Dispose();
                    subscriber.TokenSource?.Dispose();

                    CandlestickSubscriber removedSubscriber = null;

                    if(_candlestickSubscribers.TryRemove(key, out removedSubscriber))
                    {
                        _log.LogInformation($"subscriber removed for {symbol} {interval.AsString()}");
                    }
                }

                _log.LogError($"Error with candlestick websocket {symbol} {interval.AsString()}. Cache will be clear");

                _onCandlestickError?.Invoke(symbol, interval);
            }
        }

        private static string GetKey(string symbol, CandlestickInterval interval)
        {
            return $"{symbol}{interval.AsString()}";
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

            _symbolsReConnectionTimer = new Timer(async s => await SubscribeToSymbols(reConnect: true), null,
                TimeSpan.FromMinutes(WEBSOCKET_LIFE_TIME_IN_MINUTES),
                TimeSpan.FromMinutes(WEBSOCKET_LIFE_TIME_IN_MINUTES));
        }

        private void UserDataReConnectionTimerInitialize()
        {
            _userDataReConnectionTimer?.Dispose();

            _userDataReConnectionTimer = new Timer(async s => await SubscribeToUserData(reConnect: true), null,
                TimeSpan.FromMinutes(WEBSOCKET_LIFE_TIME_IN_MINUTES),
                TimeSpan.FromMinutes(WEBSOCKET_LIFE_TIME_IN_MINUTES));
        }

        private void CandlestickReConnectionTimerInitialize(CandlestickSubscriber subscriber)
        {
            subscriber.CandlestickReConnectionTimer?.Dispose();

            subscriber.CandlestickReConnectionTimer = new Timer(async(object state) =>
            {
                var _subscriber = state as CandlestickSubscriber;
                await SubscribeToCandlestick(_subscriber.Symbol, _subscriber.Interval, reConnect: true);
            }, 
            subscriber,
            TimeSpan.FromMinutes(WEBSOCKET_LIFE_TIME_IN_MINUTES),
            TimeSpan.FromMinutes(WEBSOCKET_LIFE_TIME_IN_MINUTES));
        }

        #endregion
    }
}
