namespace ApiService.Common.Providers;

public interface IBlockchainDataProvider
{
    Task<IEnumerable<TokenBalance>> GetWalletBalancesAsync(
        string walletAddress,
        string[]? chains = null,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<TokenTransaction>> GetWalletTransactionsAsync(
        string walletAddress,
        string chain,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default);

    Task<decimal?> GetTokenPriceAsync(
        string tokenAddress,
        string chain,
        CancellationToken cancellationToken = default);

    string ProviderName { get; }
}

public record TokenBalance(
    string Chain,
    string TokenAddress,
    string TokenSymbol,
    string TokenName,
    int TokenDecimals,
    decimal Balance,
    decimal? BalanceUsd
);

public record TokenTransaction(
    string TransactionHash,
    string Chain,
    DateTime TransactionDate,
    string FromAddress,
    string ToAddress,
    string TokenSymbol,
    decimal Amount,
    decimal? AmountUsd,
    decimal? Fee,
    decimal? FeeUsd,
    string Status
);
