using ApiService.Common.Database;
using ApiService.Common.Database.Entities;
using ApiService.Features.Clients.Delete;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace ApiService.Tests.Features.Clients.Delete;

public class DeleteClientHandlerTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<ILogger<DeleteClientHandler>> _loggerMock;
    private readonly DeleteClientHandler _handler;

    public DeleteClientHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new TestApplicationDbContext(options);
        _loggerMock = new Mock<ILogger<DeleteClientHandler>>();
        _handler = new DeleteClientHandler(_context, _loggerMock.Object);
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
    public async Task Handle_ValidCommand_DeletesClient()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var client = new Client
        {
            Id = clientId,
            Name = "Test Client",
            Email = "test@example.com",
            Status = "Active",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Clients.Add(client);
        await _context.SaveChangesAsync();

        var command = new DeleteClientCommand(clientId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var deletedClient = await _context.Clients.FindAsync(clientId);
        deletedClient.Should().BeNull();
    }

    [Fact]
    public async Task Handle_NonExistentClient_ReturnsFailure()
    {
        // Arrange
        var command = new DeleteClientCommand(Guid.NewGuid());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not found");
    }

    [Fact]
    public async Task Handle_ClientWithActiveAllocations_ReturnsFailure()
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

        var allocation = new ClientAssetAllocation
        {
            Id = Guid.NewGuid(),
            ClientId = clientId,
            AssetType = "Wallet",
            AssetId = walletId,
            AllocationType = "Percentage",
            AllocationValue = 50,
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
            EndDate = null, // Active allocation
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Clients.Add(client);
        _context.CustodyWallets.Add(wallet);
        _context.ClientAssetAllocations.Add(allocation);
        await _context.SaveChangesAsync();

        var command = new DeleteClientCommand(clientId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("active allocations");
    }

    [Fact]
    public async Task Handle_ClientWithEndedAllocations_DeletesSuccessfully()
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

        var allocation = new ClientAssetAllocation
        {
            Id = Guid.NewGuid(),
            ClientId = clientId,
            AssetType = "Wallet",
            AssetId = walletId,
            AllocationType = "Percentage",
            AllocationValue = 50,
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30)),
            EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)), // Ended allocation
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Clients.Add(client);
        _context.CustodyWallets.Add(wallet);
        _context.ClientAssetAllocations.Add(allocation);
        await _context.SaveChangesAsync();

        var command = new DeleteClientCommand(clientId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var deletedClient = await _context.Clients.FindAsync(clientId);
        deletedClient.Should().BeNull();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
