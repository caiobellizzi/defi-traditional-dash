# SDK Integration Guide

This document provides detailed information about integrating Moralis and Pluggy SDKs into the DeFi-Traditional Finance Dashboard.

**Note**: Use **context7** tool to fetch the latest SDK documentation when implementing.

---

## 🔗 Moralis SDK Integration (Blockchain Data)

### Overview

Moralis provides unified blockchain data across multiple chains. We use it for:
- Wallet balance tracking (all tokens, all chains)
- Transaction history
- Token prices
- NFT data (future)

### Installation (.NET 9)

```bash
# Moralis SDK for .NET
dotnet add package Moralis

# Or add to .csproj
<PackageReference Include="Moralis" Version="2.*" />
```

### Configuration

**appsettings.json**:
```json
{
  "Moralis": {
    "ApiKey": "YOUR_MORALIS_API_KEY",
    "ServerUrl": "https://deep-index.moralis.io/api/v2",
    "Chains": [
      "ethereum",
      "polygon",
      "bsc",
      "arbitrum",
      "avalanche",
      "optimism",
      "fantom"
    ],
    "RequestTimeout": 30000,
    "MaxRetries": 3
  }
}
```

### Interface Definition

```csharp
// Common/Providers/IBlockchainDataProvider.cs
public interface IBlockchainDataProvider
{
    /// <summary>
    /// Get all token balances for a wallet address across all configured chains
    /// </summary>
    Task<IEnumerable<WalletBalance>> GetWalletBalancesAsync(
        string walletAddress,
        CancellationToken ct = default);

    /// <summary>
    /// Get transaction history for a wallet
    /// </summary>
    Task<IEnumerable<TokenTransfer>> GetWalletTransactionsAsync(
        string walletAddress,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken ct = default);

    /// <summary>
    /// Get current token price in USD
    /// </summary>
    Task<decimal?> GetTokenPriceAsync(
        string tokenAddress,
        string chain,
        CancellationToken ct = default);

    /// <summary>
    /// Get list of supported blockchain chains
    /// </summary>
    Task<IEnumerable<string>> GetSupportedChainsAsync(CancellationToken ct = default);

    /// <summary>
    /// Provider name (e.g., "Moralis")
    /// </summary>
    string ProviderName { get; }
}
```

### Implementation Example

```csharp
// Common/Providers/MoralisProvider.cs
using Moralis;
using Moralis.Web3Api.Models;

public class MoralisProvider : IBlockchainDataProvider
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<MoralisProvider> _logger;
    private readonly string _apiKey;
    private readonly string[] _supportedChains;

    public string ProviderName => "Moralis";

    public MoralisProvider(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<MoralisProvider> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
        _apiKey = configuration["Moralis:ApiKey"] ?? throw new InvalidOperationException("Moralis API key not configured");
        _supportedChains = configuration.GetSection("Moralis:Chains").Get<string[]>() ?? new[] { "ethereum" };

        // Initialize Moralis
        MoralisClient.Initialize(_apiKey);
    }

    public async Task<IEnumerable<WalletBalance>> GetWalletBalancesAsync(
        string walletAddress,
        CancellationToken ct = default)
    {
        var balances = new List<WalletBalance>();

        try
        {
            foreach (var chain in _supportedChains)
            {
                _logger.LogInformation("Fetching balances for {Address} on {Chain}", walletAddress, chain);

                // Get native token balance (ETH, BNB, etc.)
                var nativeBalance = await MoralisClient.Web3Api.Account.GetNativeBalance(
                    address: walletAddress,
                    chain: ParseChain(chain));

                if (nativeBalance?.Balance != null && decimal.Parse(nativeBalance.Balance) > 0)
                {
                    balances.Add(new WalletBalance
                    {
                        Chain = chain,
                        TokenAddress = null, // Native token
                        TokenSymbol = GetNativeTokenSymbol(chain),
                        TokenName = GetNativeTokenName(chain),
                        TokenDecimals = 18,
                        Balance = decimal.Parse(nativeBalance.Balance) / (decimal)Math.Pow(10, 18),
                        LastUpdated = DateTime.UtcNow
                    });
                }

                // Get ERC20/BEP20 token balances
                var tokenBalances = await MoralisClient.Web3Api.Account.GetTokenBalances(
                    address: walletAddress,
                    chain: ParseChain(chain));

                if (tokenBalances != null)
                {
                    foreach (var token in tokenBalances)
                    {
                        if (decimal.Parse(token.Balance ?? "0") > 0)
                        {
                            balances.Add(new WalletBalance
                            {
                                Chain = chain,
                                TokenAddress = token.TokenAddress,
                                TokenSymbol = token.Symbol,
                                TokenName = token.Name,
                                TokenDecimals = int.Parse(token.Decimals ?? "18"),
                                Balance = decimal.Parse(token.Balance) / (decimal)Math.Pow(10, int.Parse(token.Decimals ?? "18")),
                                LastUpdated = DateTime.UtcNow
                            });
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching wallet balances for {Address}", walletAddress);
            throw;
        }

        return balances;
    }

    public async Task<IEnumerable<TokenTransfer>> GetWalletTransactionsAsync(
        string walletAddress,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken ct = default)
    {
        var transactions = new List<TokenTransfer>();

        try
        {
            foreach (var chain in _supportedChains)
            {
                var transfers = await MoralisClient.Web3Api.Account.GetTokenTransfers(
                    address: walletAddress,
                    chain: ParseChain(chain),
                    fromBlock: fromDate.HasValue ? GetBlockNumberFromDate(fromDate.Value, chain) : null,
                    toBlock: toDate.HasValue ? GetBlockNumberFromDate(toDate.Value, chain) : null);

                if (transfers != null)
                {
                    transactions.AddRange(transfers.Select(t => new TokenTransfer
                    {
                        Chain = chain,
                        TransactionHash = t.TransactionHash,
                        FromAddress = t.FromAddress,
                        ToAddress = t.ToAddress,
                        TokenSymbol = t.Symbol,
                        Amount = decimal.Parse(t.Value ?? "0") / (decimal)Math.Pow(10, int.Parse(t.Decimals ?? "18")),
                        BlockTimestamp = DateTime.Parse(t.BlockTimestamp),
                        Direction = DetermineDirection(walletAddress, t.FromAddress, t.ToAddress)
                    }));
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching transactions for {Address}", walletAddress);
            throw;
        }

        return transactions.OrderByDescending(t => t.BlockTimestamp);
    }

    public async Task<decimal?> GetTokenPriceAsync(
        string tokenAddress,
        string chain,
        CancellationToken ct = default)
    {
        try
        {
            var price = await MoralisClient.Web3Api.Token.GetTokenPrice(
                address: tokenAddress,
                chain: ParseChain(chain));

            return price?.UsdPrice != null ? decimal.Parse(price.UsdPrice) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching price for token {Token} on {Chain}", tokenAddress, chain);
            return null;
        }
    }

    public Task<IEnumerable<string>> GetSupportedChainsAsync(CancellationToken ct = default)
    {
        return Task.FromResult<IEnumerable<string>>(_supportedChains);
    }

    // Helper methods
    private static ChainList ParseChain(string chain) => chain.ToLowerInvariant() switch
    {
        "ethereum" => ChainList.eth,
        "polygon" => ChainList.polygon,
        "bsc" => ChainList.bsc,
        "arbitrum" => ChainList.arbitrum,
        "avalanche" => ChainList.avalanche,
        "optimism" => ChainList.optimism,
        "fantom" => ChainList.fantom,
        _ => ChainList.eth
    };

    private static string GetNativeTokenSymbol(string chain) => chain.ToLowerInvariant() switch
    {
        "ethereum" => "ETH",
        "polygon" => "MATIC",
        "bsc" => "BNB",
        "arbitrum" => "ETH",
        "avalanche" => "AVAX",
        "optimism" => "ETH",
        "fantom" => "FTM",
        _ => "ETH"
    };

    private static string GetNativeTokenName(string chain) => chain.ToLowerInvariant() switch
    {
        "ethereum" => "Ethereum",
        "polygon" => "Polygon",
        "bsc" => "BNB Smart Chain",
        "arbitrum" => "Arbitrum",
        "avalanche" => "Avalanche",
        "optimism" => "Optimism",
        "fantom" => "Fantom",
        _ => "Ethereum"
    };

    private static string DetermineDirection(string walletAddress, string from, string to)
    {
        if (from.Equals(walletAddress, StringComparison.OrdinalIgnoreCase))
            return "OUT";
        if (to.Equals(walletAddress, StringComparison.OrdinalIgnoreCase))
            return "IN";
        return "INTERNAL";
    }

    private string? GetBlockNumberFromDate(DateTime date, string chain)
    {
        // TODO: Implement block number lookup by timestamp
        // This would require a separate API call to get block by timestamp
        return null;
    }
}
```

### Models

```csharp
// Common/Providers/Models/WalletBalance.cs
public class WalletBalance
{
    public string Chain { get; set; } = string.Empty;
    public string? TokenAddress { get; set; } // NULL for native
    public string TokenSymbol { get; set; } = string.Empty;
    public string? TokenName { get; set; }
    public int TokenDecimals { get; set; }
    public decimal Balance { get; set; }
    public decimal? BalanceUSD { get; set; }
    public DateTime LastUpdated { get; set; }
}

// Common/Providers/Models/TokenTransfer.cs
public class TokenTransfer
{
    public string Chain { get; set; } = string.Empty;
    public string TransactionHash { get; set; } = string.Empty;
    public string FromAddress { get; set; } = string.Empty;
    public string ToAddress { get; set; } = string.Empty;
    public string TokenSymbol { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime BlockTimestamp { get; set; }
    public string Direction { get; set; } = string.Empty; // IN, OUT, INTERNAL
}
```

### Registration (Program.cs)

```csharp
// Register Moralis provider
builder.Services.AddHttpClient();
builder.Services.AddScoped<IBlockchainDataProvider, MoralisProvider>();

// Or with configuration switch
var blockchainProvider = builder.Configuration["BlockchainProvider"];
if (blockchainProvider == "Moralis")
{
    builder.Services.AddScoped<IBlockchainDataProvider, MoralisProvider>();
}
```

### Getting API Key

1. Go to https://moralis.io
2. Sign up for free account
3. Create new project
4. Copy API key from project settings
5. Add to appsettings.json or environment variables

### Rate Limits

- Free tier: 40,000 requests/month
- Compute units vary by endpoint
- Implement exponential backoff for 429 errors

---

## 🏦 Pluggy SDK Integration (OpenFinance)

### Overview

Pluggy provides OpenFinance (Open Banking) connectivity for Brazilian financial institutions. We use it for:
- Bank account data
- Investment account data
- Credit card information
- Transaction history

### Installation (.NET 9)

```bash
# Pluggy SDK for .NET
dotnet add package Pluggy.SDK

# Or add to .csproj
<PackageReference Include="Pluggy.SDK" Version="1.*" />
```

### Configuration

**appsettings.json**:
```json
{
  "Pluggy": {
    "ClientId": "YOUR_PLUGGY_CLIENT_ID",
    "ClientSecret": "YOUR_PLUGGY_CLIENT_SECRET",
    "BaseUrl": "https://api.pluggy.ai",
    "Environment": "sandbox", // or "production"
    "RequestTimeout": 30000,
    "ConnectWidgetUrl": "https://connect.pluggy.ai"
  }
}
```

### Interface Definition

```csharp
// Common/Providers/IOpenFinanceProvider.cs
public interface IOpenFinanceProvider
{
    /// <summary>
    /// Create a connect token for Pluggy widget
    /// </summary>
    Task<string> CreateConnectTokenAsync(CancellationToken ct = default);

    /// <summary>
    /// Get all accounts for an item (connection)
    /// </summary>
    Task<IEnumerable<AccountSummary>> GetAccountsAsync(
        string itemId,
        CancellationToken ct = default);

    /// <summary>
    /// Get account balance
    /// </summary>
    Task<IEnumerable<AccountBalance>> GetAccountBalanceAsync(
        string accountId,
        CancellationToken ct = default);

    /// <summary>
    /// Get account transactions
    /// </summary>
    Task<IEnumerable<AccountTransaction>> GetAccountTransactionsAsync(
        string accountId,
        DateTime from,
        DateTime to,
        CancellationToken ct = default);

    /// <summary>
    /// Update/refresh item data
    /// </summary>
    Task<bool> UpdateItemAsync(string itemId, CancellationToken ct = default);

    /// <summary>
    /// Delete item connection
    /// </summary>
    Task<bool> DeleteItemAsync(string itemId, CancellationToken ct = default);

    /// <summary>
    /// Provider name
    /// </summary>
    string ProviderName { get; }
}
```

### Implementation Example

```csharp
// Common/Providers/PluggyProvider.cs
using Pluggy.SDK;
using Pluggy.SDK.Model;

public class PluggyProvider : IOpenFinanceProvider
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<PluggyProvider> _logger;
    private readonly PluggyClient _pluggyClient;

    public string ProviderName => "Pluggy";

    public PluggyProvider(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<PluggyProvider> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;

        var clientId = configuration["Pluggy:ClientId"] ?? throw new InvalidOperationException("Pluggy Client ID not configured");
        var clientSecret = configuration["Pluggy:ClientSecret"] ?? throw new InvalidOperationException("Pluggy Client Secret not configured");

        _pluggyClient = new PluggyClient(clientId, clientSecret);
    }

    public async Task<string> CreateConnectTokenAsync(CancellationToken ct = default)
    {
        try
        {
            var connectToken = await _pluggyClient.CreateConnectTokenAsync();
            return connectToken.AccessToken;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating Pluggy connect token");
            throw;
        }
    }

    public async Task<IEnumerable<AccountSummary>> GetAccountsAsync(
        string itemId,
        CancellationToken ct = default)
    {
        try
        {
            var accounts = await _pluggyClient.FetchAccountsAsync(itemId);

            return accounts.Select(a => new AccountSummary
            {
                AccountId = a.Id,
                ItemId = itemId,
                Type = a.Type,
                Subtype = a.Subtype,
                Number = a.Number,
                Name = a.Name,
                Balance = a.Balance,
                CurrencyCode = a.CurrencyCode
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching accounts for item {ItemId}", itemId);
            throw;
        }
    }

    public async Task<IEnumerable<AccountBalance>> GetAccountBalanceAsync(
        string accountId,
        CancellationToken ct = default)
    {
        try
        {
            var account = await _pluggyClient.FetchAccountAsync(accountId);

            return new[]
            {
                new AccountBalance
                {
                    AccountId = accountId,
                    BalanceType = "CURRENT",
                    Currency = account.CurrencyCode ?? "BRL",
                    Amount = account.Balance ?? 0,
                    LastUpdated = DateTime.UtcNow
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching balance for account {AccountId}", accountId);
            throw;
        }
    }

    public async Task<IEnumerable<AccountTransaction>> GetAccountTransactionsAsync(
        string accountId,
        DateTime from,
        DateTime to,
        CancellationToken ct = default)
    {
        try
        {
            var transactions = await _pluggyClient.FetchTransactionsAsync(
                accountId,
                from,
                to);

            return transactions.Select(t => new AccountTransaction
            {
                TransactionId = t.Id,
                AccountId = accountId,
                Description = t.Description,
                Amount = t.Amount,
                Date = t.Date,
                Category = t.Category,
                Type = t.Type,
                CurrencyCode = t.CurrencyCode
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching transactions for account {AccountId}", accountId);
            throw;
        }
    }

    public async Task<bool> UpdateItemAsync(string itemId, CancellationToken ct = default)
    {
        try
        {
            await _pluggyClient.UpdateItemAsync(itemId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating item {ItemId}", itemId);
            return false;
        }
    }

    public async Task<bool> DeleteItemAsync(string itemId, CancellationToken ct = default)
    {
        try
        {
            await _pluggyClient.DeleteItemAsync(itemId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting item {ItemId}", itemId);
            return false;
        }
    }
}
```

### Models

```csharp
// Common/Providers/Models/AccountSummary.cs
public class AccountSummary
{
    public string AccountId { get; set; } = string.Empty;
    public string ItemId { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // BANK, INVESTMENT, CREDIT
    public string? Subtype { get; set; }
    public string? Number { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public string CurrencyCode { get; set; } = "BRL";
}

// Common/Providers/Models/AccountBalance.cs
public class AccountBalance
{
    public string AccountId { get; set; } = string.Empty;
    public string BalanceType { get; set; } = string.Empty; // AVAILABLE, CURRENT, LIMIT
    public string Currency { get; set; } = "BRL";
    public decimal Amount { get; set; }
    public DateTime LastUpdated { get; set; }
}

// Common/Providers/Models/AccountTransaction.cs
public class AccountTransaction
{
    public string TransactionId { get; set; } = string.Empty;
    public string AccountId { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public string? Category { get; set; }
    public string Type { get; set; } = string.Empty; // DEBIT, CREDIT
    public string CurrencyCode { get; set; } = "BRL";
}
```

### Getting API Credentials

1. Go to https://pluggy.ai
2. Sign up for developer account
3. Create sandbox project
4. Get Client ID and Client Secret
5. Add to appsettings.json or environment variables

### Pluggy Connect Widget (Frontend)

```typescript
// features/accounts/components/PluggyConnect.tsx
import { useEffect, useState } from 'react';

export const PluggyConnect = ({ onSuccess }: { onSuccess: (itemId: string) => void }) => {
  const [connectToken, setConnectToken] = useState<string | null>(null);

  useEffect(() => {
    // Fetch connect token from backend
    fetch('/api/traditional/accounts/connect', { method: 'POST' })
      .then(res => res.json())
      .then(data => setConnectToken(data.connectToken));
  }, []);

  useEffect(() => {
    if (!connectToken) return;

    const script = document.createElement('script');
    script.src = 'https://connect.pluggy.ai/v2.0.0/pluggy-connect.js';
    script.async = true;
    document.body.appendChild(script);

    script.onload = () => {
      // @ts-ignore
      const pluggyConnect = new PluggyConnect({
        connectToken,
        includeSandbox: true,
        onSuccess: (itemData: any) => {
          console.log('Connected:', itemData);
          onSuccess(itemData.item.id);
        },
        onError: (error: any) => {
          console.error('Connection error:', error);
        },
      });

      pluggyConnect.init();
    };

    return () => {
      document.body.removeChild(script);
    };
  }, [connectToken, onSuccess]);

  return (
    <div id="pluggy-connect-container">
      {!connectToken && <p>Loading Pluggy Connect...</p>}
    </div>
  );
};
```

---

## 📝 Next Steps

1. **Use context7** to get latest SDK documentation:
   - Moralis API reference
   - Pluggy API reference

2. **Set up API keys**:
   - Moralis API key (from dashboard)
   - Pluggy Client ID & Secret (from dashboard)

3. **Test implementations**:
   - Test Moralis with sample wallet addresses
   - Test Pluggy with sandbox accounts

4. **Implement error handling**:
   - Rate limiting
   - Retry logic with exponential backoff
   - Graceful degradation

5. **Monitor usage**:
   - Track API call counts
   - Log errors and response times
   - Set up alerts for quota limits

---

**Documentation Version**: 1.0.0
**Last Updated**: 2025-10-12
