using System.Net;
using System.Net.Http.Json;
using ApiService.Common.Database.Entities;
using FluentAssertions;

namespace ApiService.IntegrationTests.Endpoints;

/// <summary>
/// Integration tests for Allocation endpoints
/// </summary>
public class AllocationEndpointsTests : IntegrationTestBase
{
    public AllocationEndpointsTests(TestWebApplicationFactory factory) : base(factory)
    {
    }

    #region Create Allocation Tests

    [Fact]
    public async Task CreateAllocation_WithValidPercentageData_ReturnsCreated()
    {
        // Arrange
        var client = await CreateTestClient("Test Client", "test@example.com");
        var wallet = await CreateTestWallet("0x123456789", "Test Wallet");

        var command = new
        {
            clientId = client.Id,
            assetType = "Wallet",
            assetId = wallet.Id,
            allocationType = "Percentage",
            allocationValue = 50.0m,
            startDate = DateTime.UtcNow,
            notes = "50% allocation"
        };

        // Act
        var response = await PostAsJsonAsync("/api/allocations", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var result = await DeserializeResponse<CreateAllocationResponse>(response);
        result.Should().NotBeNull();
        result!.Id.Should().NotBeEmpty();

        // Verify in database
        var db = GetDbContext();
        var allocation = await db.ClientAssetAllocations.FindAsync(result.Id);
        allocation.Should().NotBeNull();
        allocation!.ClientId.Should().Be(client.Id);
        allocation.AssetType.Should().Be("Wallet");
        allocation.AllocationType.Should().Be("Percentage");
        allocation.AllocationValue.Should().Be(50.0m);
    }

    [Fact]
    public async Task CreateAllocation_WithValidFixedAmountData_ReturnsCreated()
    {
        // Arrange
        var client = await CreateTestClient("Test Client", "test@example.com");
        var wallet = await CreateTestWallet("0x987654321", "Test Wallet");

        var command = new
        {
            clientId = client.Id,
            assetType = "Wallet",
            assetId = wallet.Id,
            allocationType = "FixedAmount",
            allocationValue = 1000.50m,
            startDate = DateTime.UtcNow,
            notes = "Fixed $1000.50 allocation"
        };

        // Act
        var response = await PostAsJsonAsync("/api/allocations", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var result = await DeserializeResponse<CreateAllocationResponse>(response);
        result.Should().NotBeNull();

        // Verify in database
        var db = GetDbContext();
        var allocation = await db.ClientAssetAllocations.FindAsync(result!.Id);
        allocation.Should().NotBeNull();
        allocation!.AllocationType.Should().Be("FixedAmount");
        allocation.AllocationValue.Should().Be(1000.50m);
    }

    [Fact]
    public async Task CreateAllocation_WithNonExistentClient_ReturnsBadRequest()
    {
        // Arrange
        var wallet = await CreateTestWallet("0xABC123", "Test Wallet");
        var nonExistentClientId = Guid.NewGuid();

        var command = new
        {
            clientId = nonExistentClientId,
            assetType = "Wallet",
            assetId = wallet.Id,
            allocationType = "Percentage",
            allocationValue = 50.0m,
            startDate = DateTime.UtcNow
        };

        // Act
        var response = await PostAsJsonAsync("/api/allocations", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateAllocation_WithInvalidPercentage_ReturnsBadRequest()
    {
        // Arrange
        var client = await CreateTestClient("Test Client", "test@example.com");
        var wallet = await CreateTestWallet("0xDEF456", "Test Wallet");

        var command = new
        {
            clientId = client.Id,
            assetType = "Wallet",
            assetId = wallet.Id,
            allocationType = "Percentage",
            allocationValue = 150.0m, // Invalid: > 100%
            startDate = DateTime.UtcNow
        };

        // Act
        var response = await PostAsJsonAsync("/api/allocations", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateAllocation_WithNegativeAmount_ReturnsBadRequest()
    {
        // Arrange
        var client = await CreateTestClient("Test Client", "test@example.com");
        var wallet = await CreateTestWallet("0xGHI789", "Test Wallet");

        var command = new
        {
            clientId = client.Id,
            assetType = "Wallet",
            assetId = wallet.Id,
            allocationType = "FixedAmount",
            allocationValue = -100.0m, // Invalid: negative
            startDate = DateTime.UtcNow
        };

        // Act
        var response = await PostAsJsonAsync("/api/allocations", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Get Client Allocations Tests

    [Fact]
    public async Task GetClientAllocations_WithExistingAllocations_ReturnsAllocations()
    {
        // Arrange
        var client = await CreateTestClient("Test Client", "test@example.com");
        var wallet1 = await CreateTestWallet("0xWallet1", "Wallet 1");
        var wallet2 = await CreateTestWallet("0xWallet2", "Wallet 2");

        await CreateTestAllocation(client.Id, "Wallet", wallet1.Id, "Percentage", 40.0m);
        await CreateTestAllocation(client.Id, "Wallet", wallet2.Id, "Percentage", 60.0m);

        // Act
        var response = await GetAsync($"/api/clients/{client.Id}/allocations");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var allocations = await response.Content.ReadFromJsonAsync<List<AllocationDto>>();
        allocations.Should().NotBeNull();
        allocations!.Should().HaveCount(2);
        allocations.Should().Contain(a => a.AllocationValue == 40.0m);
        allocations.Should().Contain(a => a.AllocationValue == 60.0m);
    }

    [Fact]
    public async Task GetClientAllocations_WithNoAllocations_ReturnsEmptyList()
    {
        // Arrange
        var client = await CreateTestClient("Test Client", "test@example.com");

        // Act
        var response = await GetAsync($"/api/clients/{client.Id}/allocations");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var allocations = await response.Content.ReadFromJsonAsync<List<AllocationDto>>();
        allocations.Should().NotBeNull();
        allocations!.Should().BeEmpty();
    }

    [Fact]
    public async Task GetClientAllocations_WithNonExistentClient_ReturnsNotFound()
    {
        // Arrange
        var nonExistentClientId = Guid.NewGuid();

        // Act
        var response = await GetAsync($"/api/clients/{nonExistentClientId}/allocations");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetClientAllocations_OnlyReturnsActiveAllocations()
    {
        // Arrange
        var client = await CreateTestClient("Test Client", "test@example.com");
        var wallet = await CreateTestWallet("0xWallet", "Test Wallet");

        // Create one active and one ended allocation
        await CreateTestAllocation(client.Id, "Wallet", wallet.Id, "Percentage", 50.0m, endDate: null);
        await CreateTestAllocation(client.Id, "Wallet", wallet.Id, "Percentage", 30.0m, endDate: DateTime.UtcNow.AddDays(-1));

        // Act
        var response = await GetAsync($"/api/clients/{client.Id}/allocations");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var allocations = await response.Content.ReadFromJsonAsync<List<AllocationDto>>();
        allocations.Should().NotBeNull();
        allocations!.Should().HaveCount(1);
        allocations.First().AllocationValue.Should().Be(50.0m);
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

    private async Task<CustodyWallet> CreateTestWallet(string address, string label)
    {
        var db = GetDbContext();
        var wallet = new CustodyWallet
        {
            Id = Guid.NewGuid(),
            WalletAddress = address,
            Label = label,
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

    #endregion

    #region DTOs

    private record CreateAllocationResponse(Guid Id);

    private record AllocationDto(
        Guid Id,
        Guid ClientId,
        string AssetType,
        Guid AssetId,
        string AllocationType,
        decimal AllocationValue,
        DateTime StartDate,
        DateTime? EndDate,
        string? Notes
    );

    #endregion
}
