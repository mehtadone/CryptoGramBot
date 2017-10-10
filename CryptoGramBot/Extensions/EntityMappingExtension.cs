using AutoMapper;
using BittrexSharp.Domain;
using CryptoGramBot.Models;
using Deposit = CryptoGramBot.Models.Deposit;
using ITrade = Poloniex.TradingTools.ITrade;
using OpenOrder = CryptoGramBot.Models.OpenOrder;
using Trade = CryptoGramBot.Models.Trade;
using Withdrawal = CryptoGramBot.Models.Withdrawal;

namespace CryptoGramBot.Extensions
{
    public static class EntityMappingExtension
    {
        public static void MapEntities(this IMapperConfigurationExpression config)
        {
            config.CreateMap<HistoricOrder, Trade>()
                .ForMember(x => x.ExchangeId, d => d.MapFrom(src => src.OrderUuid));

            config.CreateMap<BittrexSharp.Domain.OpenOrder, OpenOrder>();

            config.CreateMap<Poloniex.TradingTools.IOrder, OpenOrder>()
                .ForMember(x => x.OrderUuid, d => d.MapFrom(src => src.IdOrder))
                .ForMember(x => x.Price, d => d.MapFrom(src => src.PricePerCoin))
                .ForMember(x => x.Limit, d => d.MapFrom(src => src.PricePerCoin));

            config.CreateMap<ITrade, Trade>()
                .ForMember(x => x.ExchangeId, d => d.MapFrom(src => src.GlobalTradeId))
                .ForMember(x => x.TimeStamp, d => d.MapFrom(src => src.Time))
                .ForMember(x => x.Limit, d => d.MapFrom(src => src.PricePerCoin));

            config.CreateMap<CurrencyBalance, WalletBalance>();

            config.CreateMap<Poloniex.WalletTools.Withdrawal, Withdrawal>()
                .ForMember(x => x.TransactionId, d => d.MapFrom(src => src.Id))
                .ForMember(x => x.Id, opt => opt.Ignore());
            config.CreateMap<Poloniex.WalletTools.Deposit, Deposit>()
                .ForMember(x => x.Id, opt => opt.Ignore());

            config.CreateMap<HistoricWithdrawal, Withdrawal>()
                .ForMember(x => x.TransactionId, d => d.MapFrom(src => src.TxId))
                .ForMember(x => x.Id, opt => opt.Ignore())
                .ForMember(x => x.Time, d => d.MapFrom(src => src.Opened))
                .ForMember(x => x.Cost, d => d.MapFrom(src => src.TxCost));

            config.CreateMap<HistoricDeposit, Deposit>()
                .ForMember(x => x.Id, opt => opt.Ignore())
                .ForMember(x => x.TransactionId, d => d.MapFrom(src => src.TxId))
                .ForMember(x => x.Time, d => d.MapFrom(src => src.Opened))
                .ForMember(x => x.Address, d => d.MapFrom(src => src.Address));
        }
    }
}