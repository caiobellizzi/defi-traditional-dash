using ApiService.Common.Database;
using ApiService.Common.Database.Entities;
using ApiService.Features.Clients.Update;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace ApiService.Tests.Features.Clients.Update;

public class UpdateClientHandlerTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<ILogger<UpdateClientHandler>> _loggerMock;
    private readonly UpdateClientHandler _handler;

    public UpdateClientHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new TestApplicationDbContext(options);
        _loggerMock = new Mock<ILogger<UpdateClientHandler>>();
        _handler = new UpdateClientHandler(_context, _loggerMock.Object);
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
    public async Task Handle_ValidCommand_UpdatesClient()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var existingClient = new Client
        {
            Id = clientId,
            Name = "Old Name",
            Email = "old@example.com",
            Status = "Active",
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = DateTime.UtcNow.AddDays(-1)
        };
        _context.Clients.Add(existingClient);
        await _context.SaveChangesAsync();

        var command = new UpdateClientCommand(
            Id: clientId,
            Name: "New Name",
            Email: "new@example.com",
            Document: "123456789",
            PhoneNumber: "+1234567890",
            Status: "Active",
            Notes: "Updated notes"
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var client = await _context.Clients.FindAsync(clientId);
        client.Should().NotBeNull();
        client!.Name.Should().Be("New Name");
        client.Email.Should().Be("new@example.com");
        client.Document.Should().Be("123456789");
        client.PhoneNumber.Should().Be("+1234567890");
    }

    [Fact]
    public async Task Handle_NonExistentClient_ReturnsFailure()
    {
        // Arrange
        var command = new UpdateClientCommand(
            Id: Guid.NewGuid(),
            Name: "Test",
            Email: "test@example.com",
            Document: null,
            PhoneNumber: null,
            Status: "Active",
            Notes: null
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not found");
    }

    [Fact]
    public async Task Handle_DuplicateEmail_ReturnsFailure()
    {
        // Arrange
        var client1Id = Guid.NewGuid();
        var client2Id = Guid.NewGuid();

        _context.Clients.AddRange(
            new Client
            {
                Id = client1Id,
                Name = "Client 1",
                Email = "client1@example.com",
                Status = "Active",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Client
            {
                Id = client2Id,
                Name = "Client 2",
                Email = "client2@example.com",
                Status = "Active",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        );
        await _context.SaveChangesAsync();

        var command = new UpdateClientCommand(
            Id: client1Id,
            Name: "Client 1 Updated",
            Email: "client2@example.com", // Duplicate email
            Document: null,
            PhoneNumber: null,
            Status: "Active",
            Notes: null
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("email already exists");
    }

    [Fact]
    public async Task Handle_DuplicateDocument_ReturnsFailure()
    {
        // Arrange
        var client1Id = Guid.NewGuid();
        var client2Id = Guid.NewGuid();

        _context.Clients.AddRange(
            new Client
            {
                Id = client1Id,
                Name = "Client 1",
                Email = "client1@example.com",
                Document = "111111111",
                Status = "Active",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Client
            {
                Id = client2Id,
                Name = "Client 2",
                Email = "client2@example.com",
                Document = "222222222",
                Status = "Active",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        );
        await _context.SaveChangesAsync();

        var command = new UpdateClientCommand(
            Id: client1Id,
            Name: "Client 1 Updated",
            Email: "client1@example.com",
            Document: "222222222", // Duplicate document
            PhoneNumber: null,
            Status: "Active",
            Notes: null
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("document already exists");
    }

    [Fact]
    public async Task Handle_SanitizesNotesField()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var existingClient = new Client
        {
            Id = clientId,
            Name = "Test Client",
            Email = "test@example.com",
            Status = "Active",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Clients.Add(existingClient);
        await _context.SaveChangesAsync();

        var command = new UpdateClientCommand(
            Id: clientId,
            Name: "Test Client",
            Email: "test@example.com",
            Document: null,
            PhoneNumber: null,
            Status: "Active",
            Notes: "<script>alert('XSS')</script>This is a note"
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var client = await _context.Clients.FindAsync(clientId);
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
