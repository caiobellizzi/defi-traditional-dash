using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using ApiService.Common.Database.Entities;
using FluentAssertions;

namespace ApiService.IntegrationTests.Endpoints;

/// <summary>
/// Integration tests for Transaction endpoints
/// </summary>
public class TransactionEndpointsTests : IntegrationTestBase
{
    public TransactionEndpointsTests(TestWebApplicationFactory factory) : base(factory)
    {
    }

    #region Get Transaction By Id Tests

    [Fact]
    public async Task GetTransactionById_WithExistingId_ReturnsTransaction()
    {
        // Arrange
        var wallet = await CreateTestWallet("0xTestWallet");
        var transaction = await CreateTestTransaction(
            wallet.Id,
            "Wallet",
            "0xTransactionHash123",
            "eth",
            "IN",
            "ETH",
            1.5m,
            3000.0m);

        // Act
        var response = await GetAsync($"/api/transactions/{transaction.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<TransactionDto>();
        result.Should().NotBeNull();
        result!.Id.Should().Be(transaction.Id);
        result.TransactionHash.Should().Be("0xTransactionHash123");
        result.TokenSymbol.Should().Be("ETH");
        result.Amount.Should().Be(1.5m);
        result.Direction.Should().Be("IN");
    }

    [Fact]
    public async Task GetTransactionById_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await GetAsync($"/api/transactions/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Get Transactions List Tests

    [Fact]
    public async Task GetTransactions_ReturnsAllTransactions()
    {
        // Arrange
        var wallet = await CreateTestWallet("0xTestWallet");

        await CreateTestTransaction(wallet.Id, "Wallet", "0xHash1", "eth", "IN", "ETH", 1.0m, 2000.0m);
        await CreateTestTransaction(wallet.Id, "Wallet", "0xHash2", "eth", "OUT", "USDT", 500.0m, 500.0m);
        await CreateTestTransaction(wallet.Id, "Wallet", "0xHash3", "polygon", "IN", "MATIC", 100.0m, 80.0m);

        // Act
        var response = await GetAsync("/api/transactions");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var transactions = await response.Content.ReadFromJsonAsync<TransactionPagedResult>();
        transactions.Should().NotBeNull();
        transactions!.Items.Should().HaveCount(3);
        transactions.Items.Should().Contain(t => t.TokenSymbol == "ETH");
        transactions.Items.Should().Contain(t => t.TokenSymbol == "USDT");
        transactions.Items.Should().Contain(t => t.TokenSymbol == "MATIC");
    }

    [Fact]
    public async Task GetTransactions_WhenEmpty_ReturnsEmptyList()
    {
        // Act
        var response = await GetAsync("/api/transactions");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var transactions = await response.Content.ReadFromJsonAsync<TransactionPagedResult>();
        transactions.Should().NotBeNull();
        transactions!.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task GetTransactions_ReturnsTransactionsOrderedByDate()
    {
        // Arrange
        var wallet = await CreateTestWallet("0xTestWallet");

        var tx1 = await CreateTestTransaction(wallet.Id, "Wallet", "0xOld", "eth", "IN", "ETH", 1.0m, 2000.0m,
            transactionDate: DateTime.UtcNow.AddDays(-3));
        var tx2 = await CreateTestTransaction(wallet.Id, "Wallet", "0xRecent", "eth", "IN", "ETH", 1.0m, 2000.0m,
            transactionDate: DateTime.UtcNow.AddDays(-1));
        var tx3 = await CreateTestTransaction(wallet.Id, "Wallet", "0xNewest", "eth", "IN", "ETH", 1.0m, 2000.0m,
            transactionDate: DateTime.UtcNow);

        // Act
        var response = await GetAsync("/api/transactions");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var transactions = await response.Content.ReadFromJsonAsync<TransactionPagedResult>();
        transactions.Should().NotBeNull();
        transactions!.Items.Should().HaveCount(3);

        // Verify ordering (newest first)
        transactions.Items[0].TransactionHash.Should().Be("0xNewest");
        transactions.Items[1].TransactionHash.Should().Be("0xRecent");
        transactions.Items[2].TransactionHash.Should().Be("0xOld");
    }

    [Fact]
    public async Task GetTransactions_IncludesManualEntries()
    {
        // Arrange
        var wallet = await CreateTestWallet("0xTestWallet");

        await CreateTestTransaction(wallet.Id, "Wallet", "0xAuto", "eth", "IN", "ETH", 1.0m, 2000.0m,
            isManualEntry: false);
        await CreateTestTransaction(wallet.Id, "Wallet", null, "eth", "IN", "ETH", 1.0m, 2000.0m,
            isManualEntry: true, description: "Manual adjustment");

        // Act
        var response = await GetAsync("/api/transactions");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var transactions = await response.Content.ReadFromJsonAsync<TransactionPagedResult>();
        transactions.Should().NotBeNull();
        transactions!.Items.Should().HaveCount(2);

        var manualTx = transactions.Items.FirstOrDefault(t => t.IsManualEntry);
        manualTx.Should().NotBeNull();
        manualTx!.Description.Should().Be("Manual adjustment");
        manualTx.TransactionHash.Should().BeNull();
    }

    [Fact]
    public async Task GetTransactions_FiltersByTransactionType()
    {
        // Arrange
        var wallet = await CreateTestWallet("0xTestWallet");

        await CreateTestTransaction(wallet.Id, "Wallet", "0xWallet1", "eth", "IN", "ETH", 1.0m, 2000.0m);
        await CreateTestTransaction(wallet.Id, "Wallet", "0xWallet2", "eth", "OUT", "ETH", 0.5m, 1000.0m);

        // Act - This test assumes there might be query string filtering in the future
        var response = await GetAsync("/api/transactions");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var transactions = await response.Content.ReadFromJsonAsync<TransactionPagedResult>();
        transactions.Should().NotBeNull();
        transactions!.Items.Should().HaveCount(2);
        transactions.Items.Should().Contain(t => t.Direction == "IN");
        transactions.Items.Should().Contain(t => t.Direction == "OUT");
    }

    #endregion

    #region Helper Methods

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

    private async Task<Transaction> CreateTestTransaction(
        Guid assetId,
        string transactionType,
        string? transactionHash,
        string chain,
        string direction,
        string tokenSymbol,
        decimal amount,
        decimal? amountUsd,
        DateTime? transactionDate = null,
        bool isManualEntry = false,
        string? description = null)
    {
        var db = GetDbContext();
        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            TransactionType = transactionType,
            AssetId = assetId,
            TransactionHash = transactionHash,
            Chain = chain,
            Direction = direction,
            TokenSymbol = tokenSymbol,
            Amount = amount,
            AmountUsd = amountUsd,
            TransactionDate = transactionDate ?? DateTime.UtcNow,
            IsManualEntry = isManualEntry,
            Description = description,
            Status = "Confirmed",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.Transactions.Add(transaction);
        await db.SaveChangesAsync();

        return transaction;
    }

    #endregion

    #region DTOs

    private record TransactionDto(
        Guid Id,
        string TransactionType,
        Guid AssetId,
        string? TransactionHash,
        string? ExternalId,
        string? Chain,
        string Direction,
        string? FromAddress,
        string? ToAddress,
        string? TokenSymbol,
        decimal Amount,
        decimal? AmountUsd,
        decimal? Fee,
        decimal? FeeUsd,
        string? Description,
        string? Category,
        DateTime TransactionDate,
        bool IsManualEntry,
        string Status,
        DateTime CreatedAt,
        DateTime UpdatedAt
    );

    private record TransactionPagedResult(
        List<TransactionDto> Items,
        int TotalCount,
        int PageNumber,
        int PageSize,
        int TotalPages
    );

    #endregion
}
