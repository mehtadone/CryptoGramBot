using AutoMapper;
using Bittrex;
using CryptoGramBot.Models;
using ITrade = Poloniex.TradingTools.ITrade;
using Trade = CryptoGramBot.Models.Trade;

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
        }
    }
}