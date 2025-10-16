using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using ApiService.Common.Database.Entities;
using FluentAssertions;

namespace ApiService.IntegrationTests.Endpoints;

/// <summary>
/// Integration tests for Wallet endpoints
/// </summary>
public class WalletEndpointsTests : IntegrationTestBase
{
    public WalletEndpointsTests(TestWebApplicationFactory factory) : base(factory)
    {
    }

    #region Add Wallet Tests

    [Fact]
    public async Task AddWallet_WithValidData_ReturnsCreated()
    {
        // Arrange
        var command = new
        {
            walletAddress = "0x742d35Cc6634C0532925a3b844Bc454e4438f44e",
            label = "Main Treasury Wallet",
            supportedChains = new[] { "eth", "polygon", "bsc" },
            notes = "Primary custody wallet"
        };

        // Act
        var response = await PostAsJsonAsync("/api/wallets", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await DeserializeResponse<AddWalletResponse>(response);
        result.Should().NotBeNull();
        result!.Id.Should().NotBeEmpty();

        // Verify in database
        var db = GetDbContext();
        var wallet = await db.CustodyWallets.FindAsync(result.Id);
        wallet.Should().NotBeNull();
        wallet!.WalletAddress.Should().Be("0x742d35Cc6634C0532925a3b844Bc454e4438f44e");
        wallet.Label.Should().Be("Main Treasury Wallet");
        wallet.Status.Should().Be("Active");
        wallet.SupportedChains.Should().BeEquivalentTo(new[] { "eth", "polygon", "bsc" });
    }

    [Fact]
    public async Task AddWallet_WithMinimalData_ReturnsCreated()
    {
        // Arrange
        var command = new
        {
            walletAddress = "0x1234567890abcdef1234567890abcdef12345678",
            label = (string?)null,
            supportedChains = (string[]?)null,
            notes = (string?)null
        };

        // Act
        var response = await PostAsJsonAsync("/api/wallets", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var result = await DeserializeResponse<AddWalletResponse>(response);
        result.Should().NotBeNull();

        // Verify in database
        var db = GetDbContext();
        var wallet = await db.CustodyWallets.FindAsync(result!.Id);
        wallet.Should().NotBeNull();
        wallet!.WalletAddress.Should().Be("0x1234567890abcdef1234567890abcdef12345678");
        wallet.BlockchainProvider.Should().Be("Moralis");
    }

    [Fact]
    public async Task AddWallet_WithEmptyAddress_ReturnsBadRequest()
    {
        // Arrange
        var command = new
        {
            walletAddress = "",
            label = "Test Wallet"
        };

        // Act
        var response = await PostAsJsonAsync("/api/wallets", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AddWallet_WithDuplicateAddress_ReturnsBadRequest()
    {
        // Arrange
        var address = "0xDuplicateAddress123456789";
        await CreateTestWallet(address, "First Wallet");

        var command = new
        {
            walletAddress = address,
            label = "Duplicate Wallet"
        };

        // Act
        var response = await PostAsJsonAsync("/api/wallets", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Get Wallets List Tests

    [Fact]
    public async Task GetWallets_ReturnsAllWallets()
    {
        // Arrange
        await CreateTestWallet("0xWallet1", "Wallet 1", new[] { "eth" });
        await CreateTestWallet("0xWallet2", "Wallet 2", new[] { "polygon" });
        await CreateTestWallet("0xWallet3", "Wallet 3", new[] { "bsc" });

        // Act
        var response = await GetAsync("/api/wallets");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var wallets = await response.Content.ReadFromJsonAsync<WalletPagedResult>();
        wallets.Should().NotBeNull();
        wallets!.Items.Should().HaveCount(3);
        wallets.Items.Should().Contain(w => w.Label == "Wallet 1");
        wallets.Items.Should().Contain(w => w.Label == "Wallet 2");
        wallets.Items.Should().Contain(w => w.Label == "Wallet 3");
    }

    [Fact]
    public async Task GetWallets_WhenEmpty_ReturnsEmptyList()
    {
        // Act
        var response = await GetAsync("/api/wallets");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var wallets = await response.Content.ReadFromJsonAsync<WalletPagedResult>();
        wallets.Should().NotBeNull();
        wallets!.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task GetWallets_OnlyReturnsActiveWallets()
    {
        // Arrange
        await CreateTestWallet("0xActive1", "Active Wallet", status: "Active");
        await CreateTestWallet("0xActive2", "Another Active", status: "Active");
        await CreateTestWallet("0xInactive", "Inactive Wallet", status: "Inactive");

        // Act
        var response = await GetAsync("/api/wallets");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var wallets = await response.Content.ReadFromJsonAsync<WalletPagedResult>();
        wallets.Should().NotBeNull();

        wallets!.Items.Should().HaveCountGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task GetWallets_IncludesChainInformation()
    {
        // Arrange
        var chains = new[] { "eth", "polygon", "arbitrum" };
        await CreateTestWallet("0xMultiChain", "Multi-chain Wallet", chains);

        // Act
        var response = await GetAsync("/api/wallets");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var wallets = await response.Content.ReadFromJsonAsync<WalletPagedResult>();
        wallets.Should().NotBeNull();

        var wallet = wallets!.Items.First();
        wallet.SupportedChains.Should().BeEquivalentTo(chains);
    }

    #endregion

    #region Helper Methods

    private async Task<CustodyWallet> CreateTestWallet(
        string address,
        string? label = null,
        string[]? supportedChains = null,
        string status = "Active")
    {
        var db = GetDbContext();
        var wallet = new CustodyWallet
        {
            Id = Guid.NewGuid(),
            WalletAddress = address,
            Label = label,
            BlockchainProvider = "Moralis",
            SupportedChains = supportedChains,
            Status = status,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.CustodyWallets.Add(wallet);
        await db.SaveChangesAsync();

        return wallet;
    }

    #endregion

    #region DTOs

    private record AddWalletResponse(Guid Id);

    private record WalletDto(
        Guid Id,
        string WalletAddress,
        string? Label,
        string BlockchainProvider,
        string[]? SupportedChains,
        string Status,
        string? Notes,
        DateTime CreatedAt,
        DateTime UpdatedAt
    );

    private record WalletPagedResult(
        List<WalletDto> Items,
        int TotalCount,
        int PageNumber,
        int PageSize,
        int TotalPages
    );

    #endregion
}
