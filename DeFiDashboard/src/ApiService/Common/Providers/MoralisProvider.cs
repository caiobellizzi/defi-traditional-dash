namespace ApiService.Common.Providers;

public class MoralisProvider : IBlockchainDataProvider
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<MoralisProvider> _logger;
    private readonly string _apiKey;

    public string ProviderName => "Moralis";

    public MoralisProvider(HttpClient httpClient, IConfiguration configuration, ILogger<MoralisProvider> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _apiKey = configuration["ExternalProviders:Moralis:ApiKey"]
            ?? throw new InvalidOperationException("Moralis API key not configured");

        _httpClient.BaseAddress = new Uri(
            configuration["ExternalProviders:Moralis:BaseUrl"]
            ?? "https://deep-index.moralis.io/api/v2.2");
        _httpClient.DefaultRequestHeaders.Add("X-API-Key", _apiKey);
    }

    public async Task<IEnumerable<TokenBalance>> GetWalletBalancesAsync(
        string walletAddress,
        string[]? chains = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var supportedChains = chains ?? new[] { "eth", "polygon", "bsc" };
            var balances = new List<TokenBalance>();

            foreach (var chain in supportedChains)
            {
                try
                {
                    var response = await _httpClient.GetAsync(
                        $"/wallets/{walletAddress}/tokens?chain={chain}",
                        cancellationToken);

                    if (response.IsSuccessStatusCode)
                    {
                        var data = await response.Content.ReadFromJsonAsync<MoralisTokenBalanceResponse>(cancellationToken);
                        if (data?.Result != null)
                        {
                            balances.AddRange(data.Result.Select(t => new TokenBalance(
                                Chain: chain,
                                TokenAddress: t.TokenAddress ?? "native",
                                TokenSymbol: t.Symbol ?? "Unknown",
                                TokenName: t.Name ?? "Unknown",
                                TokenDecimals: t.Decimals ?? 18,
                                Balance: ParseBalance(t.Balance, t.Decimals ?? 18),
                                BalanceUsd: t.UsdValue
                            )));
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to get balances for chain {Chain}", chain);
                }
            }

            return balances;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting wallet balances from Moralis");
            return Array.Empty<TokenBalance>();
        }
    }

    public async Task<IEnumerable<TokenTransaction>> GetWalletTransactionsAsync(
        string walletAddress,
        string chain,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var url = $"/wallets/{walletAddress}/history?chain={chain}";

            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            var data = await response.Content.ReadFromJsonAsync<MoralisTransactionResponse>(cancellationToken);

            return data?.Result?.Select(t => new TokenTransaction(
                TransactionHash: t.TransactionHash ?? "",
                Chain: chain,
                TransactionDate: t.BlockTimestamp,
                FromAddress: t.FromAddress ?? "",
                ToAddress: t.ToAddress ?? "",
                TokenSymbol: t.TokenSymbol ?? "Unknown",
                Amount: ParseBalance(t.Value, t.TokenDecimals ?? 18),
                AmountUsd: t.ValueUsd,
                Fee: ParseBalance(t.GasPrice, 18),
                FeeUsd: null,
                Status: "Confirmed"
            )) ?? Array.Empty<TokenTransaction>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting transactions from Moralis");
            return Array.Empty<TokenTransaction>();
        }
    }

    public async Task<decimal?> GetTokenPriceAsync(
        string tokenAddress,
        string chain,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync(
                $"/erc20/{tokenAddress}/price?chain={chain}",
                cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<MoralisPriceResponse>(cancellationToken);
                return data?.UsdPrice;
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting token price from Moralis");
            return null;
        }
    }

    private static decimal ParseBalance(string? balance, int decimals)
    {
        if (string.IsNullOrEmpty(balance)) return 0;
        if (decimal.TryParse(balance, out var parsed))
        {
            return parsed / (decimal)Math.Pow(10, decimals);
        }
        return 0;
    }

    // Moralis API response models
    private record MoralisTokenBalanceResponse(List<MoralisToken>? Result);
    private record MoralisToken(
        string? TokenAddress,
        string? Symbol,
        string? Name,
        int? Decimals,
        string? Balance,
        decimal? UsdValue
    );

    private record MoralisTransactionResponse(List<MoralisTransaction>? Result);
    private record MoralisTransaction(
        string? TransactionHash,
        DateTime BlockTimestamp,
        string? FromAddress,
        string? ToAddress,
        string? TokenSymbol,
        int? TokenDecimals,
        string? Value,
        decimal? ValueUsd,
        string? GasPrice
    );

    private record MoralisPriceResponse(decimal? UsdPrice);
}
