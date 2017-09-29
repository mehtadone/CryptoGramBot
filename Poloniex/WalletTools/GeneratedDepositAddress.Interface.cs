namespace Poloniex.WalletTools
{
    public interface IGeneratedDepositAddress
    {
        string Address { get; }
        bool IsGenerationSuccessful { get; }
    }
}