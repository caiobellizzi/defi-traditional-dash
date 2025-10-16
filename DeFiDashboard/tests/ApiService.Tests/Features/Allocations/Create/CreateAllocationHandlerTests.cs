using ApiService.Common.Database;
using ApiService.Common.Database.Entities;
using ApiService.Features.Allocations.Create;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace ApiService.Tests.Features.Allocations.Create;

public class CreateAllocationHandlerTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<ILogger<CreateAllocationHandler>> _loggerMock;
    private readonly CreateAllocationHandler _handler;

    public CreateAllocationHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new TestApplicationDbContext(options);
        _loggerMock = new Mock<ILogger<CreateAllocationHandler>>();
        _handler = new CreateAllocationHandler(_context, _loggerMock.Object);
    }

    private class TestApplicationDbContext : ApplicationDbContext
    {
        public TestApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<PerformanceMetric>()
                .Ignore(p => p.MetricsData);

            modelBuilder.Entity<RebalancingAlert>()
                .Ignore(r => r.AlertData);

            modelBuilder.Entity<TransactionAudit>()
                .Ignore(t => t.OldData)
                .Ignore(t => t.NewData);
        }
    }

    [Fact]
    public async Task Handle_ValidPercentageAllocation_CreatesAllocation()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var walletId = Guid.NewGuid();

        var client = new Client
        {
            Id = clientId,
            Name = "Test Client",
            Email = "test@example.com",
            Status = "Active",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var wallet = new CustodyWallet
        {
            Id = walletId,
            Address = "0x123",
            Chain = "Ethereum",
            Label = "Test Wallet",
            Status = "Active",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Clients.Add(client);
        _context.CustodyWallets.Add(wallet);
        await _context.SaveChangesAsync();

        var command = new CreateAllocationCommand(
            ClientId: clientId,
            AssetType: "Wallet",
            AssetId: walletId,
            AllocationType: "Percentage",
            AllocationValue: 50m,
            StartDate: DateOnly.FromDateTime(DateTime.UtcNow),
            Notes: "Test allocation"
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        var allocation = await _context.ClientAssetAllocations.FindAsync(result.Value);
        allocation.Should().NotBeNull();
        allocation!.AllocationValue.Should().Be(50m);
        allocation.AllocationType.Should().Be("Percentage");
    }

    [Fact]
    public async Task Handle_PercentageExceeds100_ReturnsFailure()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var walletId = Guid.NewGuid();

        var client = new Client
        {
            Id = clientId,
            Name = "Test Client",
            Email = "test@example.com",
            Status = "Active",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var wallet = new CustodyWallet
        {
            Id = walletId,
            Address = "0x123",
            Chain = "Ethereum",
            Label = "Test Wallet",
            Status = "Active",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Create existing allocation
        var existingAllocation = new ClientAssetAllocation
        {
            Id = Guid.NewGuid(),
            ClientId = Guid.NewGuid(), // Different client
            AssetType = "Wallet",
            AssetId = walletId,
            AllocationType = "Percentage",
            AllocationValue = 70m,
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
            EndDate = null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Clients.Add(client);
        _context.CustodyWallets.Add(wallet);
        _context.ClientAssetAllocations.Add(existingAllocation);
        await _context.SaveChangesAsync();

        var command = new CreateAllocationCommand(
            ClientId: clientId,
            AssetType: "Wallet",
            AssetId: walletId,
            AllocationType: "Percentage",
            AllocationValue: 40m, // Would exceed 100%
            StartDate: DateOnly.FromDateTime(DateTime.UtcNow),
            Notes: null
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("exceed 100%");
    }

    [Fact]
    public async Task Handle_InactiveClient_ReturnsFailure()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var walletId = Guid.NewGuid();

        var client = new Client
        {
            Id = clientId,
            Name = "Test Client",
            Email = "test@example.com",
            Status = "Inactive", // Inactive client
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var wallet = new CustodyWallet
        {
            Id = walletId,
            Address = "0x123",
            Chain = "Ethereum",
            Label = "Test Wallet",
            Status = "Active",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Clients.Add(client);
        _context.CustodyWallets.Add(wallet);
        await _context.SaveChangesAsync();

        var command = new CreateAllocationCommand(
            ClientId: clientId,
            AssetType: "Wallet",
            AssetId: walletId,
            AllocationType: "Percentage",
            AllocationValue: 50m,
            StartDate: DateOnly.FromDateTime(DateTime.UtcNow),
            Notes: null
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not found or inactive");
    }

    [Fact]
    public async Task Handle_DuplicateActiveAllocation_ReturnsFailure()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var walletId = Guid.NewGuid();

        var client = new Client
        {
            Id = clientId,
            Name = "Test Client",
            Email = "test@example.com",
            Status = "Active",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var wallet = new CustodyWallet
        {
            Id = walletId,
            Address = "0x123",
            Chain = "Ethereum",
            Label = "Test Wallet",
            Status = "Active",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var existingAllocation = new ClientAssetAllocation
        {
            Id = Guid.NewGuid(),
            ClientId = clientId,
            AssetType = "Wallet",
            AssetId = walletId,
            AllocationType = "Percentage",
            AllocationValue = 30m,
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
            EndDate = null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Clients.Add(client);
        _context.CustodyWallets.Add(wallet);
        _context.ClientAssetAllocations.Add(existingAllocation);
        await _context.SaveChangesAsync();

        var command = new CreateAllocationCommand(
            ClientId: clientId,
            AssetType: "Wallet",
            AssetId: walletId,
            AllocationType: "Percentage",
            AllocationValue: 20m,
            StartDate: DateOnly.FromDateTime(DateTime.UtcNow),
            Notes: null
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("already exists");
    }

    [Fact]
    public async Task Handle_FixedAmountAllocation_CreatesAllocation()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var walletId = Guid.NewGuid();

        var client = new Client
        {
            Id = clientId,
            Name = "Test Client",
            Email = "test@example.com",
            Status = "Active",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var wallet = new CustodyWallet
        {
            Id = walletId,
            Address = "0x123",
            Chain = "Ethereum",
            Label = "Test Wallet",
            Status = "Active",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Clients.Add(client);
        _context.CustodyWallets.Add(wallet);
        await _context.SaveChangesAsync();

        var command = new CreateAllocationCommand(
            ClientId: clientId,
            AssetType: "Wallet",
            AssetId: walletId,
            AllocationType: "FixedAmount",
            AllocationValue: 1000m,
            StartDate: DateOnly.FromDateTime(DateTime.UtcNow),
            Notes: "Fixed allocation"
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var allocation = await _context.ClientAssetAllocations.FindAsync(result.Value);
        allocation.Should().NotBeNull();
        allocation!.AllocationValue.Should().Be(1000m);
        allocation.AllocationType.Should().Be("FixedAmount");
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
