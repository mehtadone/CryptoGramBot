namespace Poloniex.WalletTools
{
    public interface IGeneratedDepositAddress
    {
        string Address { get; }
        string Error { get; set; }
        bool IsGenerationSuccessful { get; }
    }
}