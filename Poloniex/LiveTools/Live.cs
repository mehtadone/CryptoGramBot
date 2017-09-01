using Jojatekok.PoloniexAPI.MarketTools;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;
using WampSharp.V2;
using WampSharp.V2.Client;
using WampSharp.V2.Fluent;
using WampSharp.V2.Realm;

namespace Jojatekok.PoloniexAPI.LiveTools
{
    public class Live : ILive
    {
        private const string SubjectNameTicker = "ticker";
        private const string SubjectNameTrollbox = "trollbox";

        private readonly IDictionary<string, IDisposable> _activeSubscriptions = new Dictionary<string, IDisposable>();

        private readonly ObservableDictionary<CurrencyPair, MarketData> _tickers = new ObservableDictionary<CurrencyPair, MarketData>();

        public event EventHandler<TickerChangedEventArgs> OnTickerChanged;

        public event EventHandler<TrollboxMessageEventArgs> OnTrollboxMessage;

        public ObservableDictionary<CurrencyPair, MarketData> Tickers
        {
            get { return _tickers; }
        }

        private IDictionary<string, IDisposable> ActiveSubscriptions
        {
            get { return _activeSubscriptions; }
        }

        private IWampChannel WampChannel { get; set; }
        private Task WampChannelOpenTask { get; set; }

        public void Start()
        {
            WampChannelFactory channelFactory = new WampChannelFactory();

            WampChannel = channelFactory.ConnectToRealm("realm1")
                .WebSocketTransport(new Uri(Helper.ApiUrlWssBase))
                .JsonSerialization()
                .Build();

            WampChannel.RealmProxy.Monitor.ConnectionBroken += OnConnectionBroken;

            WampChannelOpenTask = WampChannel.Open();
        }

        public void Stop()
        {
            foreach (var subscription in ActiveSubscriptions.Values)
            {
                subscription.Dispose();
            }
            ActiveSubscriptions.Clear();

            WampChannel.Close();
        }

        public async Task SubscribeToTickerAsync()
        {
            if (!ActiveSubscriptions.ContainsKey(SubjectNameTicker))
            {
                await WampChannelOpenTask;
                ActiveSubscriptions.Add(SubjectNameTicker, WampChannel.RealmProxy.Services.GetSubject(SubjectNameTicker).Subscribe(x => ProcessMessageTicker(x.Arguments)));
            }
        }

        public async Task SubscribeToTrollboxAsync()
        {
            if (!ActiveSubscriptions.ContainsKey(SubjectNameTrollbox))
            {
                await WampChannelOpenTask;
                ActiveSubscriptions.Add(SubjectNameTrollbox, WampChannel.RealmProxy.Services.GetSubject(SubjectNameTrollbox).Subscribe(x => ProcessMessageTrollbox(x.Arguments)));
            }
        }

        private void OnConnectionBroken(object sender, WampSessionCloseEventArgs e)
        {
            if (e.CloseType != SessionCloseType.Disconnection)
            {
                var subscriptions = new string[ActiveSubscriptions.Count];
                var i = 0;
                foreach (var subjectName in ActiveSubscriptions.Keys)
                {
                    subscriptions[i] = subjectName;
                    i++;
                }
                ActiveSubscriptions.Clear();

                // Re-initialize WampChannel
                Start();

                // Re-subscribe to subjects
#pragma warning disable 4014
                for (var j = subscriptions.Length - 1; j >= 0; j--)
                {
                    var subjectName = subscriptions[j];
                    switch (subjectName)
                    {
                        case SubjectNameTicker:
                            SubscribeToTickerAsync();
                            break;

                        case SubjectNameTrollbox:
                            SubscribeToTrollboxAsync();
                            break;
                    }
                }
#pragma warning restore 4014
            }
        }

        private void ProcessMessageTicker(ISerializedValue[] arguments)
        {
            var currencyPair = CurrencyPair.Parse(arguments[0].Deserialize<string>());
            var priceLast = arguments[1].Deserialize<double>();
            var orderTopSell = arguments[2].Deserialize<double>();
            var orderTopBuy = arguments[3].Deserialize<double>();
            var priceChangePercentage = arguments[4].Deserialize<double>();
            var volume24HourBase = arguments[5].Deserialize<double>();
            var volume24HourQuote = arguments[6].Deserialize<double>();
            var isFrozenInternal = arguments[7].Deserialize<byte>();

            var marketData = new MarketData
            {
                PriceLast = priceLast,
                OrderTopSell = orderTopSell,
                OrderTopBuy = orderTopBuy,
                PriceChangePercentage = priceChangePercentage,
                Volume24HourBase = volume24HourBase,
                Volume24HourQuote = volume24HourQuote,
                IsFrozenInternal = isFrozenInternal
            };

            if (Tickers.ContainsKey(currencyPair))
            {
                Tickers[currencyPair] = marketData;
            }
            else
            {
                Tickers.Add(currencyPair, marketData);
            }

            if (OnTickerChanged != null) OnTickerChanged(this, new TickerChangedEventArgs(currencyPair, marketData));
        }

        private void ProcessMessageTrollbox(ISerializedValue[] arguments)
        {
            if (OnTrollboxMessage == null) return;

            var messageNumber = arguments[1].Deserialize<ulong>();
            var senderName = arguments[2].Deserialize<string>();
            var messageText = HttpUtility.HtmlDecode(arguments[3].Deserialize<string>());
            var senderReputation = arguments.Length >= 5 ? arguments[4].Deserialize<uint?>() : null;

            OnTrollboxMessage(this, new TrollboxMessageEventArgs(senderName, senderReputation, messageNumber, messageText));
        }
    }
}