namespace ApiService.Common.Providers;

public interface IOpenFinanceProvider
{
    /// <summary>
    /// Creates a connect token for Pluggy widget integration
    /// </summary>
    Task<string> CreateConnectTokenAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all accounts for a specific item (connection)
    /// </summary>
    Task<IEnumerable<AccountSummary>> GetAccountsAsync(
        string itemId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets current balance for a specific account
    /// </summary>
    Task<AccountBalance?> GetAccountBalanceAsync(
        string accountId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets transactions for a specific account within a date range
    /// </summary>
    Task<IEnumerable<AccountTransaction>> GetAccountTransactionsAsync(
        string accountId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets item (connection) details
    /// </summary>
    Task<ItemDetails?> GetItemDetailsAsync(
        string itemId,
        CancellationToken cancellationToken = default);

    string ProviderName { get; }
}

public record AccountSummary(
    string Id,
    string ItemId,
    string Type,
    string Subtype,
    string Name,
    string? Number,
    decimal Balance,
    string CurrencyCode,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record AccountBalance(
    string AccountId,
    decimal Available,
    decimal Current,
    string CurrencyCode,
    DateTime Date
);

public record AccountTransaction(
    string Id,
    string AccountId,
    DateTime Date,
    string Description,
    decimal Amount,
    string CurrencyCode,
    string Type,
    string? Category,
    string Status
);

public record ItemDetails(
    string Id,
    string ConnectorId,
    string Status,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    DateTime? LastUpdatedAt,
    string? Error
);
