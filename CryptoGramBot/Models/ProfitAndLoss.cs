namespace CryptoGramBot.Models
{
    public class ProfitAndLoss
    {
        public decimal AverageBuyPrice { get; set; }
        public decimal AverageSellPrice { get; set; }
        public string Base { get; set; }
        public decimal CommissionPaid { get; set; }
        public decimal ReportingProfit { get; set; }
        public string ReportingCurrency { get; set; }
        public int Id { get; set; }
        public string Pair => Base + "-" + Terms;
        public decimal Profit { get; set; }
        public decimal QuantityBought { get; set; }
        public decimal QuantitySold { get; set; }
        public decimal Remaining => QuantityBought - QuantitySold;
        public string Terms { get; set; }
        public decimal UnrealisedProfit { get; set; }
    }
}