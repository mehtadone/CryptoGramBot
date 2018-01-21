using Binance;
using Binance.Api;
using Binance.Api.WebSocket;
using Binance.Api.WebSocket.Events;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoGramBot.Services.Exchanges.WebSockets.Binance
{
    public class CustomUserDataWebSocketClient : UserDataWebSocketClient, ICustomUserDataWebSocketClient
    {
        private readonly IBinanceApi _api;        
        private readonly UserDataWebSocketClientOptions _options;

        private string _listenKey;
        private Timer _keepAliveTimer;

        public CustomUserDataWebSocketClient(IBinanceApi api, IWebSocketClient client, IOptions<UserDataWebSocketClientOptions> options = null, ILogger<UserDataWebSocketClient> logger = null) : base(api, client, options, logger)
        {
            _api = api;
            _options = options?.Value;
            
            client.Open += (s, e) =>
            {
                var period = _options?.KeepAliveTimerPeriod ?? KeepAliveTimerPeriodDefault;
                period = Math.Min(Math.Max(period, KeepAliveTimerPeriodMin), KeepAliveTimerPeriodMax);

                _keepAliveTimer = new Timer(OnKeepAliveTimer, CancellationToken.None, period, period);
            };

            client.Close += async (s, e) =>
            {
                _keepAliveTimer.Dispose();

                await _api.UserStreamCloseAsync(User, _listenKey, CancellationToken.None)
                       .ConfigureAwait(false);
            };
        }

        public override async Task SubscribeAsync(IBinanceApiUser user, Action<UserDataEventArgs> callback, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            try
            {
                _listenKey = await _api.UserStreamStartAsync(user, token)
                    .ConfigureAwait(false);

                if (string.IsNullOrWhiteSpace(_listenKey))
                    throw new Exception($"{nameof(IUserDataWebSocketClient)}: Failed to get listen key from API.");

                await SubscribeToAsync(_listenKey, callback, token)
                        .ConfigureAwait(false);
            }
            catch (OperationCanceledException) { }
            catch (Exception e)
            {
                if (!token.IsCancellationRequested)
                {
                    Logger?.LogError(e, $"{nameof(UserDataWebSocketClient)}.{nameof(SubscribeAsync)}");
                    throw;
                }
            }
        }

        private async void OnKeepAliveTimer(object state)
        {
            try
            {
                await _api.UserStreamKeepAliveAsync(User, _listenKey, (CancellationToken)state)
                    .ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Logger?.LogWarning(e, $"{nameof(UserDataWebSocketClient)}.{nameof(OnKeepAliveTimer)}: \"{e.Message}\"");
            }
        }
    }
}
