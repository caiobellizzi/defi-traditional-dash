var builder = DistributedApplication.CreateBuilder(args);

// Add PostgreSQL with health check
var postgres = builder.AddPostgres("postgres")
    .WithEnvironment("POSTGRES_DB", "defi_dashboard")
    .WithHealthCheck(); // Explicit health check for readiness

var database = postgres.AddDatabase("defi-db");

// Add API Service (waits for PostgreSQL to be ready)
var apiService = builder.AddProject<Projects.ApiService>("apiservice")
    .WithReference(database)
    .WaitFor(postgres); // Wait for PostgreSQL to be healthy

// Add Frontend with hot-reload (waits for API to be ready)
builder.AddNpmApp("frontend", "../frontend", "dev")
    .WithHttpEndpoint(port: 5173, env: "PORT")
    .WithEnvironment("VITE_API_BASE_URL", apiService.GetEndpoint("https")) // Use HTTPS
    .WithReference(apiService) // Explicit reference
    .WaitFor(apiService) // Wait for API to be healthy
    .WithExternalHttpEndpoints()
    .PublishAsDockerFile();

builder.Build().Run();
