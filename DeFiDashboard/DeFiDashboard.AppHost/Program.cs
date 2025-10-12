var builder = DistributedApplication.CreateBuilder(args);

// Add PostgreSQL (can use local or Supabase)
var postgres = builder.AddPostgres("postgres")
    .WithEnvironment("POSTGRES_DB", "defi_dashboard");

var database = postgres.AddDatabase("defi-db");

// Add API Service
var apiService = builder.AddProject<Projects.ApiService>("apiservice")
    .WithReference(database)
    .WithEnvironment("ConnectionStrings__Supabase", database);

// Add Frontend with hot-reload
builder.AddNpmApp("frontend", "../frontend", "dev")
    .WithHttpEndpoint(port: 5173, env: "PORT")
    .WithEnvironment("VITE_API_BASE_URL", apiService.GetEndpoint("http"))
    .WithExternalHttpEndpoints()
    .PublishAsDockerFile();

builder.Build().Run();
