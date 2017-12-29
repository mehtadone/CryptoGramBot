namespace Jojatekok.PoloniexAPI.WalletTools
{
    public interface IGeneratedDepositAddress
    {
        bool IsGenerationSuccessful { get; }

        string Address { get; }
    }
}
