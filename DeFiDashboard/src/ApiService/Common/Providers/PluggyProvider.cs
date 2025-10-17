using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ApiService.Common.Providers;

public class PluggyProvider : IOpenFinanceProvider
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<PluggyProvider> _logger;
    private readonly string _clientId;
    private readonly string _clientSecret;
    private string? _accessToken;
    private DateTime _tokenExpiresAt;

    public string ProviderName => "Pluggy";

    public PluggyProvider(HttpClient httpClient, IConfiguration configuration, ILogger<PluggyProvider> logger)
    {
        _httpClient = httpClient;
        _logger = logger;

        _clientId = configuration["ExternalProviders:Pluggy:ClientId"]
            ?? throw new InvalidOperationException("Pluggy ClientId not configured");
        _clientSecret = configuration["ExternalProviders:Pluggy:ClientSecret"]
            ?? throw new InvalidOperationException("Pluggy ClientSecret not configured");

        _httpClient.BaseAddress = new Uri(
            configuration["ExternalProviders:Pluggy:BaseUrl"]
            ?? "https://api.pluggy.ai");
    }

    public async Task<string> CreateConnectTokenAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await EnsureAuthenticatedAsync(cancellationToken);

            var response = await _httpClient.PostAsync("/connect_token", null, cancellationToken);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<PluggyConnectTokenResponse>(cancellationToken);

            if (result?.AccessToken == null)
            {
                throw new InvalidOperationException("Failed to create connect token");
            }

            _logger.LogInformation("Created Pluggy connect token successfully");
            return result.AccessToken;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating Pluggy connect token");
            throw;
        }
    }

    public async Task<IEnumerable<AccountSummary>> GetAccountsAsync(
        string itemId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await EnsureAuthenticatedAsync(cancellationToken);

            var response = await _httpClient.GetAsync($"/accounts?itemId={itemId}", cancellationToken);
            response.EnsureSuccessStatusCode();

            var data = await response.Content.ReadFromJsonAsync<PluggyAccountsResponse>(cancellationToken);

            return data?.Results?.Select(a => new AccountSummary(
                Id: a.Id ?? "",
                ItemId: a.ItemId ?? "",
                Type: a.Type ?? "Unknown",
                Subtype: a.Subtype ?? "",
                Name: a.Name ?? "Unknown Account",
                Number: a.Number,
                Balance: a.Balance ?? 0,
                CurrencyCode: a.CurrencyCode ?? "USD",
                CreatedAt: a.CreatedAt,
                UpdatedAt: a.UpdatedAt
            )) ?? Array.Empty<AccountSummary>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting accounts from Pluggy for item {ItemId}", itemId);
            return Array.Empty<AccountSummary>();
        }
    }

    public async Task<AccountBalance?> GetAccountBalanceAsync(
        string accountId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await EnsureAuthenticatedAsync(cancellationToken);

            var response = await _httpClient.GetAsync($"/accounts/{accountId}", cancellationToken);
            response.EnsureSuccessStatusCode();

            var account = await response.Content.ReadFromJsonAsync<PluggyAccount>(cancellationToken);

            if (account == null)
            {
                return null;
            }

            return new AccountBalance(
                AccountId: account.Id ?? "",
                Available: account.Balance ?? 0,
                Current: account.Balance ?? 0,
                CurrencyCode: account.CurrencyCode ?? "USD",
                Date: DateTime.UtcNow
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting account balance from Pluggy for account {AccountId}", accountId);
            return null;
        }
    }

    public async Task<IEnumerable<AccountTransaction>> GetAccountTransactionsAsync(
        string accountId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await EnsureAuthenticatedAsync(cancellationToken);

            var fromStr = from.ToString("yyyy-MM-dd");
            var toStr = to.ToString("yyyy-MM-dd");
            var url = $"/transactions?accountId={accountId}&from={fromStr}&to={toStr}";

            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            var data = await response.Content.ReadFromJsonAsync<PluggyTransactionsResponse>(cancellationToken);

            return data?.Results?.Select(t => new AccountTransaction(
                Id: t.Id ?? "",
                AccountId: t.AccountId ?? "",
                Date: t.Date,
                Description: t.Description ?? "",
                Amount: t.Amount ?? 0,
                CurrencyCode: t.CurrencyCode ?? "USD",
                Type: t.Type ?? "Unknown",
                Category: t.Category,
                Status: t.Status ?? "Posted"
            )) ?? Array.Empty<AccountTransaction>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting transactions from Pluggy for account {AccountId}", accountId);
            return Array.Empty<AccountTransaction>();
        }
    }

    public async Task<ItemDetails?> GetItemDetailsAsync(
        string itemId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await EnsureAuthenticatedAsync(cancellationToken);

            var response = await _httpClient.GetAsync($"/items/{itemId}", cancellationToken);
            response.EnsureSuccessStatusCode();

            var item = await response.Content.ReadFromJsonAsync<PluggyItem>(cancellationToken);

            if (item == null)
            {
                return null;
            }

            return new ItemDetails(
                Id: item.Id ?? "",
                ConnectorId: item.ConnectorId ?? "",
                Status: item.Status ?? "Unknown",
                CreatedAt: item.CreatedAt,
                UpdatedAt: item.UpdatedAt,
                LastUpdatedAt: item.LastUpdatedAt,
                Error: item.Error
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting item details from Pluggy for item {ItemId}", itemId);
            return null;
        }
    }

    private async Task EnsureAuthenticatedAsync(CancellationToken cancellationToken)
    {
        // Check if we have a valid token
        if (!string.IsNullOrEmpty(_accessToken) && DateTime.UtcNow < _tokenExpiresAt)
        {
            return;
        }

        // Get new access token
        await AuthenticateAsync(cancellationToken);
    }

    private async Task AuthenticateAsync(CancellationToken cancellationToken)
    {
        try
        {
            var authPayload = new
            {
                clientId = _clientId,
                clientSecret = _clientSecret
            };

            var content = new StringContent(
                JsonSerializer.Serialize(authPayload),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync("/auth", content, cancellationToken);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<PluggyAuthResponse>(cancellationToken);

            if (result?.AccessToken == null)
            {
                throw new InvalidOperationException("Failed to authenticate with Pluggy");
            }

            _accessToken = result.AccessToken;
            _tokenExpiresAt = DateTime.UtcNow.AddSeconds(result.ExpiresIn ?? 3600);

            // Set authorization header
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _accessToken);

            _logger.LogInformation("Successfully authenticated with Pluggy");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error authenticating with Pluggy");
            throw;
        }
    }

    // Pluggy API response models
    private record PluggyAuthResponse(
        [property: JsonPropertyName("accessToken")] string? AccessToken,
        [property: JsonPropertyName("expiresIn")] int? ExpiresIn
    );

    private record PluggyConnectTokenResponse(
        [property: JsonPropertyName("accessToken")] string? AccessToken
    );

    private record PluggyAccountsResponse(
        [property: JsonPropertyName("results")] List<PluggyAccount>? Results
    );

    private record PluggyAccount(
        [property: JsonPropertyName("id")] string? Id,
        [property: JsonPropertyName("itemId")] string? ItemId,
        [property: JsonPropertyName("type")] string? Type,
        [property: JsonPropertyName("subtype")] string? Subtype,
        [property: JsonPropertyName("name")] string? Name,
        [property: JsonPropertyName("number")] string? Number,
        [property: JsonPropertyName("balance")] decimal? Balance,
        [property: JsonPropertyName("currencyCode")] string? CurrencyCode,
        [property: JsonPropertyName("createdAt")] DateTime CreatedAt,
        [property: JsonPropertyName("updatedAt")] DateTime UpdatedAt
    );

    private record PluggyTransactionsResponse(
        [property: JsonPropertyName("results")] List<PluggyTransaction>? Results
    );

    private record PluggyTransaction(
        [property: JsonPropertyName("id")] string? Id,
        [property: JsonPropertyName("accountId")] string? AccountId,
        [property: JsonPropertyName("date")] DateTime Date,
        [property: JsonPropertyName("description")] string? Description,
        [property: JsonPropertyName("amount")] decimal? Amount,
        [property: JsonPropertyName("currencyCode")] string? CurrencyCode,
        [property: JsonPropertyName("type")] string? Type,
        [property: JsonPropertyName("category")] string? Category,
        [property: JsonPropertyName("status")] string? Status
    );

    private record PluggyItem(
        [property: JsonPropertyName("id")] string? Id,
        [property: JsonPropertyName("connectorId")] string? ConnectorId,
        [property: JsonPropertyName("status")] string? Status,
        [property: JsonPropertyName("createdAt")] DateTime CreatedAt,
        [property: JsonPropertyName("updatedAt")] DateTime UpdatedAt,
        [property: JsonPropertyName("lastUpdatedAt")] DateTime? LastUpdatedAt,
        [property: JsonPropertyName("error")] string? Error
    );
}
