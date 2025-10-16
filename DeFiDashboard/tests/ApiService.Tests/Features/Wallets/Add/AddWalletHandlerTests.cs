using ApiService.Common.Database;
using ApiService.Common.Database.Entities;
using ApiService.Features.Wallets.Add;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace ApiService.Tests.Features.Wallets.Add;

public class AddWalletHandlerTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<ILogger<AddWalletHandler>> _loggerMock;
    private readonly AddWalletHandler _handler;

    public AddWalletHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new TestApplicationDbContext(options);
        _loggerMock = new Mock<ILogger<AddWalletHandler>>();
        _handler = new AddWalletHandler(_context, _loggerMock.Object);
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
    public async Task Handle_ValidCommand_AddsWallet()
    {
        // Arrange
        var command = new AddWalletCommand(
            Address: "0x742d35Cc6634C0532925a3b844Bc9e7595f0bEb",
            Chain: "Ethereum",
            Label: "My Test Wallet",
            Notes: "Test wallet for unit testing"
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        var wallet = await _context.CustodyWallets.FindAsync(result.Value);
        wallet.Should().NotBeNull();
        wallet!.Address.Should().Be("0x742d35Cc6634C0532925a3b844Bc9e7595f0bEb");
        wallet.Chain.Should().Be("Ethereum");
        wallet.Status.Should().Be("Active");
    }

    [Fact]
    public async Task Handle_DuplicateAddress_ReturnsFailure()
    {
        // Arrange
        var existingWallet = new CustodyWallet
        {
            Id = Guid.NewGuid(),
            Address = "0x742d35Cc6634C0532925a3b844Bc9e7595f0bEb",
            Chain = "Ethereum",
            Label = "Existing Wallet",
            Status = "Active",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.CustodyWallets.Add(existingWallet);
        await _context.SaveChangesAsync();

        var command = new AddWalletCommand(
            Address: "0x742d35Cc6634C0532925a3b844Bc9e7595f0bEb",
            Chain: "Ethereum",
            Label: "New Wallet",
            Notes: null
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("already exists");
    }

    [Fact]
    public async Task Handle_NormalizesAddress()
    {
        // Arrange
        var command = new AddWalletCommand(
            Address: "0X742D35CC6634C0532925A3B844BC9E7595F0BEB", // Uppercase
            Chain: "Ethereum",
            Label: "Test Wallet",
            Notes: null
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var wallet = await _context.CustodyWallets.FindAsync(result.Value);
        wallet.Should().NotBeNull();
        // Address should be normalized (lowercase)
        wallet!.Address.Should().NotContain("X");
    }

    [Fact]
    public async Task Handle_DifferentChains_AllowsSameAddress()
    {
        // Arrange
        var existingWallet = new CustodyWallet
        {
            Id = Guid.NewGuid(),
            Address = "0x742d35Cc6634C0532925a3b844Bc9e7595f0bEb",
            Chain = "Ethereum",
            Label = "Ethereum Wallet",
            Status = "Active",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.CustodyWallets.Add(existingWallet);
        await _context.SaveChangesAsync();

        var command = new AddWalletCommand(
            Address: "0x742d35Cc6634C0532925a3b844Bc9e7595f0bEb",
            Chain: "Polygon", // Different chain
            Label: "Polygon Wallet",
            Notes: null
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert - Should succeed because different chain
        result.IsSuccess.Should().BeTrue();

        var wallets = await _context.CustodyWallets
            .Where(w => w.Address == "0x742d35Cc6634C0532925a3b844Bc9e7595f0bEb")
            .ToListAsync();
        wallets.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_SanitizesNotesField()
    {
        // Arrange
        var command = new AddWalletCommand(
            Address: "0x742d35Cc6634C0532925a3b844Bc9e7595f0bEb",
            Chain: "Ethereum",
            Label: "Test Wallet",
            Notes: "<script>alert('XSS')</script>This is a note"
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var wallet = await _context.CustodyWallets.FindAsync(result.Value);
        wallet!.Notes.Should().NotContain("<script>");
        wallet.Notes.Should().NotContain("alert");
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
