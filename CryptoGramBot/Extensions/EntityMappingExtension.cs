using AutoMapper;
using Bittrex;
using Bittrex.Data;
using CryptoGramBot.Models;
using Deposit = CryptoGramBot.Models.Deposit;
using ITrade = Poloniex.TradingTools.ITrade;
using Trade = CryptoGramBot.Models.Trade;
using Withdrawal = CryptoGramBot.Models.Withdrawal;

namespace CryptoGramBot.Extensions
{
    public static class EntityMappingExtension
    {
        public static void MapEntities(this IMapperConfigurationExpression config)
        {
            config.CreateMap<CompletedOrder, Trade>()
                .ForMember(x => x.ExchangeId, d => d.MapFrom(src => src.OrderUuid));

            config.CreateMap<ITrade, Trade>()
                .ForMember(x => x.ExchangeId, d => d.MapFrom(src => src.IdOrder))
                .ForMember(x => x.TimeStamp, d => d.MapFrom(src => src.Time))
                .ForMember(x => x.Limit, d => d.MapFrom(src => src.PricePerCoin));

            config.CreateMap<GetBalancesResponse, WalletBalance>();
            config.CreateMap<AccountBalance, WalletBalance>();

            config.CreateMap<Poloniex.WalletTools.Withdrawal, Withdrawal>();
            config.CreateMap<Poloniex.WalletTools.Deposit, Deposit>();
        }
    }
}