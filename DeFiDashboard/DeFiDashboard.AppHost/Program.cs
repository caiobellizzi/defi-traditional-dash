var builder = DistributedApplication.CreateBuilder(args);

// Use direct Supabase PostgreSQL connection (no container)
var database = builder.AddConnectionString("defi-db");

// Add API Service
var apiService = builder.AddProject<Projects.ApiService>("apiservice")
    .WithReference(database);

// Add Frontend with hot-reload (waits for API to be ready)
// NOTE: Frontend runs separately via `npm run dev` in frontend directory
// Aspire npm hosting has port detection issues with Vite
// builder.AddNpmApp("frontend", "../frontend", "dev")
//     .WithHttpEndpoint(port: 5173)
//     .WithEnvironment("VITE_API_BASE_URL", apiService.GetEndpoint("https"))
//     .WithReference(apiService)
//     .WithExternalHttpEndpoints();

builder.Build().Run();
