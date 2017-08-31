using AutoMapper;
using Bittrex;
using TeleCoinigy.Models;

namespace TeleCoinigy.Extensions
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