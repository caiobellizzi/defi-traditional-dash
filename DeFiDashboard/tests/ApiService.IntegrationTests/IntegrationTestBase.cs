using System.Net.Http.Json;
using System.Text.Json;
using ApiService.Common.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ApiService.IntegrationTests;

/// <summary>
/// Base class for integration tests providing common functionality
/// </summary>
public abstract class IntegrationTestBase : IClassFixture<TestWebApplicationFactory>, IAsyncLifetime
{
    protected readonly TestWebApplicationFactory Factory;
    protected HttpClient Client { get; private set; } = null!;
    private IServiceScope? _scope;

    protected IntegrationTestBase(TestWebApplicationFactory factory)
    {
        Factory = factory;
    }

    /// <summary>
    /// Gets a fresh database context for test operations
    /// </summary>
    protected ApplicationDbContext GetDbContext()
    {
        _scope?.Dispose();
        _scope = Factory.Services.CreateScope();
        return _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    }

    /// <summary>
    /// POST request helper with JSON serialization
    /// </summary>
    protected async Task<HttpResponseMessage> PostAsJsonAsync<T>(string url, T data)
    {
        return await Client.PostAsJsonAsync(url, data, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }

    /// <summary>
    /// PUT request helper with JSON serialization
    /// </summary>
    protected async Task<HttpResponseMessage> PutAsJsonAsync<T>(string url, T data)
    {
        return await Client.PutAsJsonAsync(url, data, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }

    /// <summary>
    /// GET request helper
    /// </summary>
    protected async Task<HttpResponseMessage> GetAsync(string url)
    {
        return await Client.GetAsync(url);
    }

    /// <summary>
    /// DELETE request helper
    /// </summary>
    protected async Task<HttpResponseMessage> DeleteAsync(string url)
    {
        return await Client.DeleteAsync(url);
    }

    /// <summary>
    /// Deserialize response content to specified type
    /// </summary>
    protected async Task<T?> DeserializeResponse<T>(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        });
    }

    /// <summary>
    /// Initialize test (called before each test)
    /// </summary>
    public virtual async Task InitializeAsync()
    {
        Client = Factory.CreateClient();
        await ResetDatabaseAsync();
    }

    /// <summary>
    /// Cleanup test resources (called after each test)
    /// </summary>
    public virtual Task DisposeAsync()
    {
        // Clean up scope
        _scope?.Dispose();
        _scope = null;
        Client?.Dispose();

        // For InMemory database, we create a new database with unique name each time
        // so no cleanup is needed between tests
        return Task.CompletedTask;
    }

    private async Task ResetDatabaseAsync()
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        const string truncateAllTables = """
            DO $$
            DECLARE
                stmt text;
            BEGIN
                FOR stmt IN
                    SELECT format('TRUNCATE TABLE %I.%I RESTART IDENTITY CASCADE;', schemaname, tablename)
                    FROM pg_tables
                    WHERE schemaname = 'dash'
                LOOP
                    EXECUTE stmt;
                END LOOP;
            END $$;
            """;

        await db.Database.ExecuteSqlRawAsync(truncateAllTables);
    }
}
