# Testing Guide

## Overview

This document provides comprehensive information about testing the DeFi-Traditional Finance Dashboard.

## Test Structure

```
DeFiDashboard/
├── tests/
│   ├── ApiService.Tests/              # Unit tests
│   │   ├── Features/
│   │   │   ├── Clients/
│   │   │   │   ├── Create/CreateClientHandlerTests.cs
│   │   │   │   ├── Update/UpdateClientHandlerTests.cs
│   │   │   │   └── Delete/DeleteClientHandlerTests.cs
│   │   │   ├── Allocations/
│   │   │   │   └── Create/CreateAllocationHandlerTests.cs
│   │   │   └── Wallets/
│   │   │       └── Add/AddWalletHandlerTests.cs
│   │   └── Common/
│   │       └── Utilities/InputSanitizerTests.cs
│   └── ApiService.IntegrationTests/   # Integration tests
│       ├── Endpoints/
│       │   ├── ClientEndpointsTests.cs
│       │   ├── WalletEndpointsTests.cs
│       │   ├── AllocationEndpointsTests.cs
│       │   ├── TransactionEndpointsTests.cs
│       │   └── PortfolioEndpointsTests.cs
│       ├── TestWebApplicationFactory.cs
│       └── IntegrationTestBase.cs
├── frontend/
│   └── e2e/                           # E2E tests (Playwright)
│       ├── clients.spec.ts
│       ├── wallets.spec.ts
│       └── portfolio.spec.ts
└── run-tests.sh                       # Test runner script
```

## Running Tests

### Quick Start

**Linux/macOS**:
```bash
./run-tests.sh
```

**Windows**:
```powershell
.\run-tests.ps1
```

### Individual Test Suites

#### Unit Tests Only
```bash
cd tests/ApiService.Tests
dotnet test --configuration Release
```

#### Integration Tests Only
```bash
cd tests/ApiService.IntegrationTests
dotnet test --configuration Release
```

#### Frontend E2E Tests
```bash
cd frontend
npm run test:e2e
```

### With Coverage
```bash
cd tests/ApiService.Tests
dotnet test --collect:"XPlat Code Coverage"
```

## Test Categories

### 1. Unit Tests

**Purpose**: Test individual handlers in isolation

**Coverage**:
- ✅ CreateClientHandler (3 tests)
- ✅ UpdateClientHandler (5 tests)
- ✅ DeleteClientHandler (4 tests)
- ✅ CreateAllocationHandler (6 tests)
- ✅ AddWalletHandler (5 tests)
- ✅ InputSanitizer (existing)

**Example Test**:
```csharp
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
}
```

### 2. Integration Tests

**Purpose**: Test complete API endpoints with real database

**Coverage**:
- ✅ Client CRUD endpoints
- ✅ Wallet CRUD endpoints
- ✅ Allocation endpoints
- ✅ Transaction endpoints
- ✅ Portfolio endpoints

**Infrastructure**:
- Uses **Testcontainers** for PostgreSQL
- Real database per test run
- Automatic cleanup

**Example Test**:
```csharp
[Fact]
public async Task GetClients_ReturnsOk()
{
    // Act
    var response = await _client.GetAsync("/api/clients");

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.OK);
    var content = await response.Content.ReadFromJsonAsync<PagedResult<ClientDto>>();
    content.Should().NotBeNull();
}
```

### 3. Frontend E2E Tests

**Purpose**: Test user workflows end-to-end

**Coverage**:
- ✅ Client management flow
- ✅ Wallet addition flow
- ✅ Allocation creation flow
- ✅ Portfolio viewing

**Example Test**:
```typescript
test('should create a new client', async ({ page }) => {
  await page.goto('/clients');
  await page.click('text=Add Client');
  await page.fill('input[name="name"]', 'John Doe');
  await page.fill('input[name="email"]', 'john@example.com');
  await page.click('button:has-text("Create Client")');
  await expect(page.locator('text=John Doe')).toBeVisible();
});
```

## Test Data

### Unit Tests

Unit tests use **in-memory database** (EF Core InMemory provider):
- Each test gets a fresh database
- Fast execution (no I/O)
- JSON properties ignored (InMemory limitation)

### Integration Tests

Integration tests use **Testcontainers PostgreSQL**:
- Real PostgreSQL 16 instance
- Automatic startup/cleanup
- Supports all database features

### Seed Data

For integration tests, seed data is created in `IntegrationTestBase`:
```csharp
protected async Task SeedTestData()
{
    var client = new Client
    {
        Id = TestClientId,
        Name = "Test Client",
        Email = "test@example.com",
        Status = "Active",
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };

    _context.Clients.Add(client);
    await _context.SaveChangesAsync();
}
```

## Coverage Goals

### Current Coverage (Estimated)

- **Handlers**: 80%+ (23 tests)
- **Endpoints**: 70%+ (integration tests)
- **Validators**: 90%+ (FluentValidation)
- **Utilities**: 95%+ (InputSanitizer)

### Coverage Report

After running `./run-tests.sh`, view coverage report:
```
open test-results/coverage-report/index.html
```

## Testing Best Practices

### 1. Test Naming Convention

```csharp
[Fact]
public async Task {MethodName}_{Scenario}_{ExpectedResult}()
{
    // Test code
}
```

Examples:
- `Handle_ValidCommand_CreatesClient`
- `Handle_DuplicateEmail_ReturnsFailure`
- `Handle_InactiveClient_ReturnsFailure`

### 2. AAA Pattern

All tests follow **Arrange, Act, Assert**:

```csharp
[Fact]
public async Task Handle_ValidCommand_CreatesClient()
{
    // Arrange
    var command = new CreateClientCommand(...);

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.IsSuccess.Should().BeTrue();
}
```

### 3. FluentAssertions

Use FluentAssertions for readable assertions:

```csharp
// Good
result.IsSuccess.Should().BeTrue();
client.Name.Should().Be("John Doe");
clients.Should().HaveCount(5);

// Avoid
Assert.True(result.IsSuccess);
Assert.Equal("John Doe", client.Name);
```

### 4. Test Isolation

- Each test should be independent
- No shared state between tests
- Use `IDisposable` for cleanup

```csharp
public class MyTests : IDisposable
{
    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
```

## Common Test Scenarios

### Testing Validation

```csharp
[Fact]
public async Task Handle_EmptyName_ReturnsFailure()
{
    // Arrange
    var command = new CreateClientCommand(
        Name: "",  // Invalid
        Email: "test@example.com",
        Document: null,
        PhoneNumber: null,
        Notes: null
    );

    // Act & Assert
    await Assert.ThrowsAsync<ValidationException>(
        () => _handler.Handle(command, CancellationToken.None)
    );
}
```

### Testing Business Rules

```csharp
[Fact]
public async Task Handle_PercentageExceeds100_ReturnsFailure()
{
    // Arrange: Create existing allocation (70%)
    var existingAllocation = new ClientAssetAllocation
    {
        AllocationValue = 70m,
        // ... other properties
    };
    _context.ClientAssetAllocations.Add(existingAllocation);
    await _context.SaveChangesAsync();

    // Try to add 40% (would exceed 100%)
    var command = new CreateAllocationCommand(
        AllocationType: "Percentage",
        AllocationValue: 40m,
        // ... other properties
    );

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.IsSuccess.Should().BeFalse();
    result.Error.Should().Contain("exceed 100%");
}
```

### Testing Security (XSS)

```csharp
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
    var client = await _context.Clients.FindAsync(result.Value);
    client!.Notes.Should().NotContain("<script>");
    client.Notes.Should().NotContain("alert");
}
```

## Troubleshooting

### Tests Fail to Build

**Issue**: Compilation errors in main project

**Solution**: Fix compilation errors first, then run tests
```bash
cd src/ApiService
dotnet build
```

### Testcontainers Timeout

**Issue**: Docker not running or slow to start

**Solution**:
1. Ensure Docker Desktop is running
2. Increase timeout in `TestWebApplicationFactory.cs`:
```csharp
private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
    .WithStartupCallback((container, ct) => container.StartAsync(ct))
    .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5432))
    .Build();
```

### InMemory Database Limitations

**Issue**: JsonDocument properties not supported

**Solution**: Use custom `TestApplicationDbContext` that ignores JSON properties:
```csharp
modelBuilder.Entity<PerformanceMetric>()
    .Ignore(p => p.MetricsData);
```

### Coverage Report Not Generated

**Issue**: ReportGenerator not installed

**Solution**:
```bash
dotnet tool install -g dotnet-reportgenerator-globaltool
```

## Continuous Integration

### GitHub Actions Example

```yaml
name: Tests

on: [push, pull_request]

jobs:
  test:
    runs-on: ubuntu-latest

    steps:
    - uses: actions/checkout@v3

    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '9.0.x'

    - name: Restore dependencies
      run: dotnet restore

    - name: Build
      run: dotnet build --no-restore

    - name: Run Unit Tests
      run: dotnet test tests/ApiService.Tests --no-build --verbosity normal

    - name: Run Integration Tests
      run: dotnet test tests/ApiService.IntegrationTests --no-build --verbosity normal

    - name: Upload Coverage
      uses: codecov/codecov-action@v3
      with:
        file: test-results/**/coverage.cobertura.xml
```

## Test Coverage Metrics

### Target Metrics

- **Line Coverage**: > 80%
- **Branch Coverage**: > 70%
- **Cyclomatic Complexity**: < 10 per method

### Viewing Metrics

```bash
# Generate detailed report
reportgenerator \
    -reports:"test-results/**/coverage.cobertura.xml" \
    -targetdir:"test-results/coverage-report" \
    -reporttypes:"Html;Badges;TextSummary"

# View report
open test-results/coverage-report/index.html
```

## Future Test Additions

### Planned Tests

- [ ] Portfolio calculation tests
- [ ] Performance metric tests
- [ ] Alert generation tests
- [ ] Export functionality tests (PDF/Excel)
- [ ] Background job tests
- [ ] Moralis provider mock tests
- [ ] Pluggy provider mock tests

### Test Helpers Needed

- [ ] Test data builders (for complex entities)
- [ ] Mock provider implementations
- [ ] Custom assertions for business rules
- [ ] Performance benchmarks

## Resources

- [xUnit Documentation](https://xunit.net/)
- [FluentAssertions Documentation](https://fluentassertions.com/)
- [Testcontainers Documentation](https://dotnet.testcontainers.org/)
- [Playwright Documentation](https://playwright.dev/)
- [EF Core Testing Guide](https://learn.microsoft.com/en-us/ef/core/testing/)

---

**Last Updated**: 2025-10-16
**Version**: 1.0.0
