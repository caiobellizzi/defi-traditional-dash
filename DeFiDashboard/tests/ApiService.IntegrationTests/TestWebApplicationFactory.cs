using ApiService.Common.Database;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Testcontainers.PostgreSql;

namespace ApiService.IntegrationTests;

/// <summary>
/// Custom WebApplicationFactory using Testcontainers for PostgreSQL
/// Provides a real PostgreSQL database for integration tests
/// </summary>
public class TestWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly Dictionary<string, string?> _originalEnv = new();
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("testdb")
        .WithUsername("test")
        .WithPassword("test")
        .Build();

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
        var connectionString = _dbContainer.GetConnectionString();
        SetEnvironmentVariable("ConnectionStrings__defi-db", connectionString);
        SetEnvironmentVariable("ASPIRE__NPGSQL__ENTITYFRAMEWORKCORE__POSTGRESQL__CONNECTIONSTRING", connectionString);
        SetEnvironmentVariable("ASPIRE__NPGSQL__ENTITYFRAMEWORKCORE__POSTGRESQL__APPLICATIONDBCONTEXT__CONNECTIONSTRING", connectionString);
    }

    public new async Task DisposeAsync()
    {
        await _dbContainer.DisposeAsync();
        foreach (var (key, value) in _originalEnv)
        {
            Environment.SetEnvironmentVariable(key, value);
        }
        await base.DisposeAsync();
    }

    private void SetEnvironmentVariable(string key, string? value)
    {
        if (!_originalEnv.ContainsKey(key))
        {
            _originalEnv[key] = Environment.GetEnvironmentVariable(key);
        }

        Environment.SetEnvironmentVariable(key, value);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Override the connection string to use the Testcontainers database
            var connectionString = _dbContainer.GetConnectionString();
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:defi-db"] = connectionString,
                ["Aspire:Npgsql:EntityFrameworkCore:PostgreSQL:ConnectionString"] = connectionString,
                ["Aspire:Npgsql:EntityFrameworkCore:PostgreSQL:ApplicationDbContext:ConnectionString"] = connectionString
            });
        });

        builder.ConfigureServices(services =>
        {
            // Replace Aspire-registered DbContext with explicit configuration for testing
            var connectionString = _dbContainer.GetConnectionString();
            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
            services.RemoveAll<ApplicationDbContext>();
            services.RemoveAll<IDbContextFactory<ApplicationDbContext>>();
            services.AddDbContextPool<ApplicationDbContext>(options => options.UseNpgsql(connectionString));
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);

        // Ensure database is created after host is built
        using (var scope = host.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Database.ExecuteSqlRaw("CREATE EXTENSION IF NOT EXISTS \"uuid-ossp\";");
            db.Database.EnsureCreated();
        }

        return host;
    }
}
