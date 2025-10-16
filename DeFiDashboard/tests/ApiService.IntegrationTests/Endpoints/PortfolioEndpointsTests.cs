using System.Net;
using System.Net.Http.Json;
using ApiService.Common.Database.Entities;
using FluentAssertions;

namespace ApiService.IntegrationTests.Endpoints;

/// <summary>
/// Integration tests for Portfolio endpoints
/// </summary>
public class PortfolioEndpointsTests : IntegrationTestBase
{
    public PortfolioEndpointsTests(TestWebApplicationFactory factory) : base(factory)
    {
    }

    #region Get Client Portfolio Tests

    [Fact]
    public async Task GetClientPortfolio_WithExistingClient_ReturnsPortfolio()
    {
        // Arrange
        var client = await CreateTestClient("Portfolio Client", "portfolio@example.com");
        var wallet = await CreateTestWallet("0xPortfolioWallet");

        // Create allocation for client
        await CreateTestAllocation(client.Id, "Wallet", wallet.Id, "Percentage", 100.0m);

        // Create wallet balances
        await CreateTestWalletBalance(wallet.Id, "ETH", "eth", 5.0m, 10000.0m);
        await CreateTestWalletBalance(wallet.Id, "USDT", "eth", 2000.0m, 2000.0m);

        // Act
        var response = await GetAsync($"/api/clients/{client.Id}/portfolio");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var portfolio = await response.Content.ReadFromJsonAsync<PortfolioDto>();
        portfolio.Should().NotBeNull();
        portfolio!.ClientId.Should().Be(client.Id);
        portfolio.ClientName.Should().Be("Portfolio Client");
    }

    [Fact]
    public async Task GetClientPortfolio_WithNonExistentClient_ReturnsNotFound()
    {
        // Arrange
        var nonExistentClientId = Guid.NewGuid();

        // Act
        var response = await GetAsync($"/api/clients/{nonExistentClientId}/portfolio");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetClientPortfolio_WithNoAllocations_ReturnsEmptyPortfolio()
    {
        // Arrange
        var client = await CreateTestClient("Empty Portfolio Client", "empty@example.com");

        // Act
        var response = await GetAsync($"/api/clients/{client.Id}/portfolio");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var portfolio = await response.Content.ReadFromJsonAsync<PortfolioDto>();
        portfolio.Should().NotBeNull();
        portfolio!.ClientId.Should().Be(client.Id);
    }

    [Fact]
    public async Task GetClientPortfolio_WithMultipleAllocations_AggregatesCorrectly()
    {
        // Arrange
        var client = await CreateTestClient("Multi Allocation Client", "multi@example.com");
        var wallet1 = await CreateTestWallet("0xWallet1");
        var wallet2 = await CreateTestWallet("0xWallet2");

        // Create allocations
        await CreateTestAllocation(client.Id, "Wallet", wallet1.Id, "Percentage", 50.0m);
        await CreateTestAllocation(client.Id, "Wallet", wallet2.Id, "Percentage", 100.0m);

        // Create balances
        await CreateTestWalletBalance(wallet1.Id, "ETH", "eth", 10.0m, 20000.0m);
        await CreateTestWalletBalance(wallet2.Id, "BTC", "eth", 0.5m, 25000.0m);

        // Act
        var response = await GetAsync($"/api/clients/{client.Id}/portfolio");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var portfolio = await response.Content.ReadFromJsonAsync<PortfolioDto>();
        portfolio.Should().NotBeNull();
        portfolio!.ClientId.Should().Be(client.Id);
    }

    [Fact]
    public async Task GetClientPortfolio_WithFixedAmountAllocation_CalculatesCorrectly()
    {
        // Arrange
        var client = await CreateTestClient("Fixed Amount Client", "fixed@example.com");
        var wallet = await CreateTestWallet("0xFixedWallet");

        // Create fixed amount allocation
        await CreateTestAllocation(client.Id, "Wallet", wallet.Id, "FixedAmount", 5000.0m);

        // Create balances
        await CreateTestWalletBalance(wallet.Id, "USDC", "eth", 10000.0m, 10000.0m);

        // Act
        var response = await GetAsync($"/api/clients/{client.Id}/portfolio");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var portfolio = await response.Content.ReadFromJsonAsync<PortfolioDto>();
        portfolio.Should().NotBeNull();
        portfolio!.ClientId.Should().Be(client.Id);
    }

    [Fact]
    public async Task GetClientPortfolio_OnlyIncludesActiveAllocations()
    {
        // Arrange
        var client = await CreateTestClient("Active Only Client", "active@example.com");
        var wallet1 = await CreateTestWallet("0xActiveWallet");
        var wallet2 = await CreateTestWallet("0xEndedWallet");

        // Create one active and one ended allocation
        await CreateTestAllocation(client.Id, "Wallet", wallet1.Id, "Percentage", 100.0m, endDate: null);
        await CreateTestAllocation(client.Id, "Wallet", wallet2.Id, "Percentage", 50.0m,
            endDate: DateTime.UtcNow.AddDays(-1));

        // Create balances for both wallets
        await CreateTestWalletBalance(wallet1.Id, "ETH", "eth", 1.0m, 2000.0m);
        await CreateTestWalletBalance(wallet2.Id, "ETH", "eth", 2.0m, 4000.0m);

        // Act
        var response = await GetAsync($"/api/clients/{client.Id}/portfolio");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var portfolio = await response.Content.ReadFromJsonAsync<PortfolioDto>();
        portfolio.Should().NotBeNull();
        // Portfolio should only include active allocation from wallet1
    }

    #endregion

    #region Helper Methods

    private async Task<Client> CreateTestClient(string name, string email)
    {
        var db = GetDbContext();
        var client = new Client
        {
            Id = Guid.NewGuid(),
            Name = name,
            Email = email,
            Status = "Active",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.Clients.Add(client);
        await db.SaveChangesAsync();

        return client;
    }

    private async Task<CustodyWallet> CreateTestWallet(string address)
    {
        var db = GetDbContext();
        var wallet = new CustodyWallet
        {
            Id = Guid.NewGuid(),
            WalletAddress = address,
            Label = "Test Wallet",
            BlockchainProvider = "Moralis",
            Status = "Active",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.CustodyWallets.Add(wallet);
        await db.SaveChangesAsync();

        return wallet;
    }

    private async Task<ClientAssetAllocation> CreateTestAllocation(
        Guid clientId,
        string assetType,
        Guid assetId,
        string allocationType,
        decimal allocationValue,
        DateTime? endDate = null)
    {
        var db = GetDbContext();
        var allocation = new ClientAssetAllocation
        {
            Id = Guid.NewGuid(),
            ClientId = clientId,
            AssetType = assetType,
            AssetId = assetId,
            AllocationType = allocationType,
            AllocationValue = allocationValue,
            StartDate = DateTime.UtcNow,
            EndDate = endDate,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.ClientAssetAllocations.Add(allocation);
        await db.SaveChangesAsync();

        return allocation;
    }

    private async Task<WalletBalance> CreateTestWalletBalance(
        Guid walletId,
        string tokenSymbol,
        string chain,
        decimal balance,
        decimal? balanceUsd)
    {
        var db = GetDbContext();
        var walletBalance = new WalletBalance
        {
            Id = Guid.NewGuid(),
            WalletId = walletId,
            TokenSymbol = tokenSymbol,
            Chain = chain,
            Balance = balance,
            BalanceUsd = balanceUsd,
            LastUpdated = DateTime.UtcNow
        };

        db.WalletBalances.Add(walletBalance);
        await db.SaveChangesAsync();

        return walletBalance;
    }

    #endregion

    #region DTOs

    private record PortfolioDto(
        Guid ClientId,
        string ClientName,
        decimal TotalValueUsd,
        List<AssetAllocationDto> Allocations,
        List<TokenBalanceDto> TokenBalances,
        DateTime LastUpdated
    );

    private record AssetAllocationDto(
        Guid AllocationId,
        string AssetType,
        Guid AssetId,
        string AssetLabel,
        string AllocationType,
        decimal AllocationValue,
        decimal CurrentValueUsd
    );

    private record TokenBalanceDto(
        string TokenSymbol,
        string Chain,
        decimal Balance,
        decimal? BalanceUsd
    );

    #endregion
}
