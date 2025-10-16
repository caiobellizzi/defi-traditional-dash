using ApiService.Common.Database;
using ApiService.Common.Database.Entities;
using ApiService.Features.Clients.Create;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace ApiService.Tests.Features.Clients.Create;

public class CreateClientHandlerTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<ILogger<CreateClientHandler>> _loggerMock;
    private readonly CreateClientHandler _handler;

    public CreateClientHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new TestApplicationDbContext(options);
        _loggerMock = new Mock<ILogger<CreateClientHandler>>();
        _handler = new CreateClientHandler(_context, _loggerMock.Object);
    }

    // Test-specific DbContext that ignores JsonDocument properties for InMemory provider
    private class TestApplicationDbContext : ApplicationDbContext
    {
        public TestApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Ignore JsonDocument properties for InMemory provider
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
    public async Task Handle_ValidCommand_CreatesClient()
    {
        // Arrange
        var command = new CreateClientCommand(
            Name: "John Doe",
            Email: "john.doe@example.com",
            Document: "12345678900",
            PhoneNumber: "+1234567890",
            Notes: "Test client"
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        var client = await _context.Clients.FindAsync(result.Value);
        client.Should().NotBeNull();
        client!.Name.Should().Be("John Doe");
        client.Email.Should().Be("john.doe@example.com");
        client.Status.Should().Be("Active");
    }

    [Fact]
    public async Task Handle_DuplicateEmail_ReturnsFailure()
    {
        // Arrange
        var existingClient = new Client
        {
            Id = Guid.NewGuid(),
            Name = "Existing User",
            Email = "duplicate@example.com",
            Status = "Active",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Clients.Add(existingClient);
        await _context.SaveChangesAsync();

        var command = new CreateClientCommand(
            Name: "New User",
            Email: "duplicate@example.com",
            Document: null,
            PhoneNumber: null,
            Notes: null
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("email already exists");
    }

    [Fact]
    public async Task Handle_SanitizesNotesField()
    {
        // Arrange
        var command = new CreateClientCommand(
            Name: "Test User",
            Email: "test@example.com",
            Document: null,
            PhoneNumber: null,
            Notes: "<script>alert('XSS')</script>This is a note"
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var client = await _context.Clients.FindAsync(result.Value);
        client!.Notes.Should().NotContain("<script>");
        client.Notes.Should().NotContain("alert");
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
