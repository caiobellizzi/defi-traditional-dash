using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using ApiService.Common.Database.Entities;
using FluentAssertions;

namespace ApiService.IntegrationTests.Endpoints;

/// <summary>
/// Integration tests for Client endpoints
/// </summary>
public class ClientEndpointsTests : IntegrationTestBase
{
    public ClientEndpointsTests(TestWebApplicationFactory factory) : base(factory)
    {
    }

    #region Create Client Tests

    [Fact]
    public async Task CreateClient_WithValidData_ReturnsCreated()
    {
        // Arrange
        var command = new
        {
            name = "John Doe",
            email = "john.doe@example.com",
            document = "123.456.789-00",
            phoneNumber = "+1234567890",
            notes = "Test client"
        };

        // Act
        var response = await PostAsJsonAsync("/api/clients", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var result = await DeserializeResponse<CreateClientResponse>(response);
        result.Should().NotBeNull();
        result!.Id.Should().NotBeEmpty();

        // Verify in database
        var db = GetDbContext();
        var client = await db.Clients.FindAsync(result.Id);
        client.Should().NotBeNull();
        client!.Name.Should().Be("John Doe");
        client.Email.Should().Be("john.doe@example.com");
        client.Status.Should().Be("Active");
    }

    [Fact]
    public async Task CreateClient_WithMissingName_ReturnsBadRequest()
    {
        // Arrange
        var command = new
        {
            name = "",
            email = "test@example.com"
        };

        // Act
        var response = await PostAsJsonAsync("/api/clients", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateClient_WithInvalidEmail_ReturnsBadRequest()
    {
        // Arrange
        var command = new
        {
            name = "John Doe",
            email = "invalid-email"
        };

        // Act
        var response = await PostAsJsonAsync("/api/clients", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Get Client By Id Tests

    [Fact]
    public async Task GetClientById_WithExistingId_ReturnsClient()
    {
        // Arrange
        var client = await CreateTestClient("Jane Smith", "jane@example.com");

        // Act
        var response = await GetAsync($"/api/clients/{client.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ClientDto>();
        result.Should().NotBeNull();
        result!.Id.Should().Be(client.Id);
        result.Name.Should().Be("Jane Smith");
        result.Email.Should().Be("jane@example.com");
    }

    [Fact]
    public async Task GetClientById_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await GetAsync($"/api/clients/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Get Clients List Tests

    [Fact]
    public async Task GetClients_ReturnsAllClients()
    {
        // Arrange
        await CreateTestClient("Client 1", "client1@example.com");
        await CreateTestClient("Client 2", "client2@example.com");
        await CreateTestClient("Client 3", "client3@example.com");

        // Act
        var response = await GetAsync("/api/clients");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var clients = await response.Content.ReadFromJsonAsync<ClientPagedResult>();
        clients.Should().NotBeNull();
        clients!.Items.Should().HaveCount(3);
        clients.Items.Should().Contain(c => c.Name == "Client 1");
        clients.Items.Should().Contain(c => c.Name == "Client 2");
        clients.Items.Should().Contain(c => c.Name == "Client 3");
    }

    [Fact]
    public async Task GetClients_WhenEmpty_ReturnsEmptyList()
    {
        // Act
        var response = await GetAsync("/api/clients");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var clients = await response.Content.ReadFromJsonAsync<ClientPagedResult>();
        clients.Should().NotBeNull();
        clients!.Items.Should().BeEmpty();
    }

    #endregion

    #region Update Client Tests

    [Fact]
    public async Task UpdateClient_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var client = await CreateTestClient("Original Name", "original@example.com");

        var updateCommand = new
        {
            name = "Updated Name",
            email = "updated@example.com",
            document = "987.654.321-00",
            phoneNumber = "+9876543210",
            notes = "Updated notes",
            status = "Active"
        };

        // Act
        var response = await PutAsJsonAsync($"/api/clients/{client.Id}", updateCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify in database
        var db = GetDbContext();
        var updatedClient = await db.Clients.FindAsync(client.Id);
        updatedClient.Should().NotBeNull();
        updatedClient!.Name.Should().Be("Updated Name");
        updatedClient.Email.Should().Be("updated@example.com");
        updatedClient.Document.Should().Be("987.654.321-00");
    }

    [Fact]
    public async Task UpdateClient_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var updateCommand = new
        {
            name = "Updated Name",
            email = "updated@example.com",
            status = "Active"
        };

        // Act
        var response = await PutAsJsonAsync($"/api/clients/{nonExistentId}", updateCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateClient_WithInvalidData_ReturnsBadRequest()
    {
        // Arrange
        var client = await CreateTestClient("Test Client", "test@example.com");

        var updateCommand = new
        {
            name = "", // Invalid: empty name
            email = "test@example.com",
            status = "Active"
        };

        // Act
        var response = await PutAsJsonAsync($"/api/clients/{client.Id}", updateCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Delete Client Tests

    [Fact]
    public async Task DeleteClient_WithExistingId_ReturnsNoContent()
    {
        // Arrange
        var client = await CreateTestClient("Client to Delete", "delete@example.com");

        // Act
        var response = await DeleteAsync($"/api/clients/{client.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify in database
        var db = GetDbContext();
        var deletedClient = await db.Clients.FindAsync(client.Id);
        deletedClient.Should().BeNull();
    }

    [Fact]
    public async Task DeleteClient_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await DeleteAsync($"/api/clients/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
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

    #endregion

    #region DTOs

    private record CreateClientResponse(Guid Id);

    private record ClientDto(
        Guid Id,
        string Name,
        string Email,
        string? Document,
        string? PhoneNumber,
        string Status,
        string? Notes,
        DateTime CreatedAt,
        DateTime UpdatedAt
    );

    private record ClientPagedResult(
        List<ClientDto> Items,
        int TotalCount,
        int PageNumber,
        int PageSize,
        int TotalPages
    );

    #endregion
}
