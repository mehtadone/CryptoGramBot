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

            public Timer CandlestickDisconnectionTimer { get; set; }

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

        private Timer _symbolsDisconnectionTimer;
        private Timer _userDataDisconnectionTimer;

        private Task _userDataSubscribeTask;
        private Task _symbolsSubscribeTask;

        private Action<OrderUpdateEventArgs> _onOrderUpdate;
        private Action<AccountUpdateEventArgs> _onAccountUpdate;
        private Action<AccountTradeUpdateEventArgs> _onAccountTradeUpdate;
        private Action<SymbolStatisticsEventArgs> _onSymbolStatisticUpdate;

        private Action<CandlestickEventArgs> _onCandlestickUpdate;

        private Action _onSymbolStatisticErrorOrDisconnect;
        private Action<string, CandlestickInterval> _onCandlestickErrorOrDisconnect;
        private Func<Task> _onUserDataErrorOrDisconnect;

        private BinanceApiUser _user;

        private SemaphoreSlim _symbolsStatisticSemaphore;
        private SemaphoreSlim _userDataSemaphore;
        private SemaphoreSlim _candlestickSemaphore;

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

            _symbolsStatisticSemaphore = new SemaphoreSlim(1, 1);
            _userDataSemaphore = new SemaphoreSlim(1, 1);
            _candlestickSemaphore = new SemaphoreSlim(1, 1);
        }

        #endregion

        #region IBinanceSubscribersService

        public async Task SymbolsStatistics(Action<SymbolStatisticsEventArgs> onUpdate, Action onError)
        {
            if (_symbolsSubscribeTask == null)
            {
                _onSymbolStatisticUpdate = onUpdate ?? throw new ArgumentException(nameof(onUpdate));
                _onSymbolStatisticErrorOrDisconnect = onError ?? throw new ArgumentException(nameof(onError));                

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
                _onUserDataErrorOrDisconnect = onError ?? throw new ArgumentException(nameof(onError));

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
                _onCandlestickErrorOrDisconnect = onError ?? throw new ArgumentException(nameof(onError));

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
                _symbolsStatisticSemaphore?.Dispose();
                _userDataSemaphore?.Dispose();
                _candlestickSemaphore?.Dispose();

                _symbolsDisconnectionTimer?.Dispose();
                _userDataDisconnectionTimer?.Dispose();

                _symbolStatisticCancellationTokenSource?.Cancel();
                _userDataCancellationTokenSource?.Cancel();

                _symbolStatisticCancellationTokenSource?.Dispose();
                _userDataCancellationTokenSource?.Dispose();

                if(_candlestickSubscribers != null)
                {
                    foreach (var item in _candlestickSubscribers)
                    {
                        item.Value.CandlestickDisconnectionTimer?.Dispose();
                        item.Value.TokenSource?.Cancel();
                        item.Value.TokenSource?.Dispose();
                    }
                }              

                _isDisposed = true;
            }
        }

        #endregion

        #region Private methods

        private async Task SubscribeToSymbols()
        {
            if (_symbolsSubscribeTask != null)
            {
                return;
            }

            try
            {
                try
                {
                    await _symbolsStatisticSemaphore.WaitAsync();

                    if(_symbolsSubscribeTask != null)
                    {
                        return;
                    }

                    _log.LogInformation($"Subscribe to symbols");

                    _symbolStatisticCancellationTokenSource?.Cancel();
                    _symbolStatisticCancellationTokenSource?.Dispose();

                    _symbolStatisticCancellationTokenSource = new CancellationTokenSource();

                    _symbolStatisticsWebSocketClient = _serviceProvider.GetService<ISymbolStatisticsWebSocketClient>();

                    _symbolsSubscribeTask = _symbolStatisticsWebSocketClient.SubscribeAsync(_onSymbolStatisticUpdate, _symbolStatisticCancellationTokenSource.Token);
                    
                    SymbolsDisconnectionTimerInitialize();
                }
                finally
                {
                    _symbolsStatisticSemaphore.Release();
                }

                await _symbolsSubscribeTask;

                _symbolsSubscribeTask = null;
            }
            catch (Exception)
            {
                _symbolsSubscribeTask = null;

                OnSymbolStatisticDisconnect(true);
            }
        }

        private async Task SubscribeToUserData()
        {
            if (_userDataSubscribeTask != null)
            {
                return;
            }

            try
            {
                try
                {
                    await _userDataSemaphore.WaitAsync();

                    if(_userDataSubscribeTask != null)
                    {
                        return;
                    }

                    _log.LogInformation("Subscribe to user data");

                    _user?.Dispose();
                    _user = null;

                    _userDataCancellationTokenSource?.Cancel();
                    _userDataCancellationTokenSource?.Dispose();

                    _userDataCancellationTokenSource = new CancellationTokenSource();

                    if (_userDataWebSocketClient != null)
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

                    UserDataDisconnectionTimerInitialize();
                }
                finally
                {
                    _userDataSemaphore.Release();
                }

                await _userDataSubscribeTask;

                _userDataSubscribeTask = null;
            }
            catch (Exception)
            {
                _userDataSubscribeTask = null;

                await OnUserDataDisconnect(true);
            }
        }

        private async Task SubscribeToCandlestick(string symbol, CandlestickInterval interval)
        {
            var key = GetKey(symbol, interval);

            if (_candlestickSubscribers.ContainsKey(key))
            {
                return;
            }

            try
            {
                var subscriber = new CandlestickSubscriber()
                {
                    Symbol = symbol,
                    Interval = interval
                };

                try
                {
                    await _candlestickSemaphore.WaitAsync();

                    if (_candlestickSubscribers.ContainsKey(key))
                    {
                        return;
                    }

                    _log.LogInformation($"Subscribe to candlestick {symbol} {interval.AsString()}");

                    _candlestickSubscribers[key] = subscriber;

                    subscriber.TokenSource = new CancellationTokenSource();
                    subscriber.CandlestickWebSocketClient = _serviceProvider.GetService<ICandlestickWebSocketClient>();
                    subscriber.SubscribeTask = subscriber.CandlestickWebSocketClient.SubscribeAsync(symbol, interval, _onCandlestickUpdate, subscriber.TokenSource.Token);

                    CandlestickDisconnectionTimerInitialize(subscriber);
                }
                finally
                {
                    _candlestickSemaphore.Release();
                }

                await subscriber.SubscribeTask;

                RemoveSubscriber(symbol, interval);
            }
            catch (Exception)
            {
                RemoveSubscriber(symbol, interval);

                OnCandlesticDisconnect(symbol, interval, true);
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

        private void OnSymbolStatisticDisconnect(bool error = false)
        {
            _symbolStatisticCancellationTokenSource?.Cancel();

            var logMessage = error ? "Error with symbol statistic websocket" : "Symbol statistic websocket disconnected!";

            _log.LogInformation($"{logMessage} Cache will be clear");

            _onSymbolStatisticErrorOrDisconnect?.Invoke();
        }

        private async Task OnUserDataDisconnect(bool error = false)
        {
            _userDataCancellationTokenSource?.Cancel();

            var logMessage = error ? "Error with user data websocket" : "User data websocket disconnected!";

            _log.LogInformation($"{logMessage} Cache will be clear");

            await _onUserDataErrorOrDisconnect?.Invoke();
        }

        private void OnCandlesticDisconnect(string symbol, CandlestickInterval interval, bool error = false)
        {
            var key = GetKey(symbol, interval);

            if (_candlestickSubscribers.ContainsKey(key))
            {
                var subscriber = _candlestickSubscribers[key];

                subscriber.CandlestickDisconnectionTimer?.Dispose();

                subscriber.TokenSource?.Cancel();
                subscriber.TokenSource?.Dispose();
            }

            var logMessage = error ? "Error with candlestick websocket" : "Candlestick websocket disconnected";

            _log.LogInformation($"{logMessage} {symbol} {interval.AsString()}. Cache will be clear");

            _onCandlestickErrorOrDisconnect(symbol, interval);
        }

        private void RemoveSubscriber(string symbol, CandlestickInterval interval)
        {
            var key = GetKey(symbol, interval);

            CandlestickSubscriber removedSubscriber = null;

            if (_candlestickSubscribers.TryRemove(key, out removedSubscriber))
            {
                _log.LogInformation($"subscriber removed for {symbol} {interval.AsString()}");
            }
        }

        private void SymbolsDisconnectionTimerInitialize()
        {
            _symbolsDisconnectionTimer?.Dispose();

            _symbolsDisconnectionTimer = new Timer(s => OnSymbolStatisticDisconnect(), null,
                TimeSpan.FromMinutes(WEBSOCKET_LIFE_TIME_IN_MINUTES),
                TimeSpan.FromMilliseconds(-1));
        }

        private void UserDataDisconnectionTimerInitialize()
        {
            _userDataDisconnectionTimer?.Dispose();

            _userDataDisconnectionTimer = new Timer(async s => await OnUserDataDisconnect(), null,
                TimeSpan.FromMinutes(WEBSOCKET_LIFE_TIME_IN_MINUTES),
                TimeSpan.FromMilliseconds(-1));
        }

        private void CandlestickDisconnectionTimerInitialize(CandlestickSubscriber subscriber)
        {
            subscriber.CandlestickDisconnectionTimer?.Dispose();

            subscriber.CandlestickDisconnectionTimer = new Timer((object state) =>
            {
                var _subscriber = state as CandlestickSubscriber;

                OnCandlesticDisconnect(_subscriber.Symbol, _subscriber.Interval);
            }, 
            subscriber,
            TimeSpan.FromMinutes(WEBSOCKET_LIFE_TIME_IN_MINUTES),
            TimeSpan.FromMilliseconds(-1));
        }

        #endregion
    }
}
