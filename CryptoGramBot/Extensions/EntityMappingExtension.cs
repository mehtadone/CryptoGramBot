using AutoMapper;
using Bittrex;
using CryptoGramBot.Models;
using Jojatekok.PoloniexAPI.TradingTools;
using Trade = CryptoGramBot.Models.Trade;

namespace CryptoGramBot.Extensions
{
    public static class EntityMappingExtension
    {
        public static void MapEntities(this IMapperConfigurationExpression config)
        {
            config.CreateMap<CompletedOrder, Trade>()
                .ForMember(x => x.Id, d => d.MapFrom(src => src.OrderUuid))
                .ForMember(x => x.Cost, d => d.MapFrom(src => src.Price));

            config.CreateMap<ITrade, Trade>()
                .ForMember(x => x.Id, d => d.MapFrom(src => src.IdOrder))
                .ForMember(x => x.TimeStamp, d => d.MapFrom(src => src.Time))
                .ForMember(x => x.Limit, d => d.MapFrom(src => src.PricePerCoin));

            config.CreateMap<GetBalancesResponse, WalletBalance>();
        }
    }
}