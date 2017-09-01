using AutoMapper;
using Bittrex;
using CryptoGramBot.Models;

namespace CryptoGramBot.Extensions
{
    public static class EntityMappingExtension
    {
        public static void MapEntities(this IMapperConfigurationExpression config)
        {
            config.CreateMap<CompletedOrder, Trade>()
                .ForMember(x => x.Id, d => d.MapFrom(src => src.OrderUuid))
                .ForMember(x => x.Cost, d => d.MapFrom(src => src.Price));
        }
    }
}