using ApiService.BackgroundJobs;
using ApiService.Common.Behaviors;
using ApiService.Common.Database;
using ApiService.Common.Hubs;
using ApiService.Common.Middleware;
using ApiService.Common.Providers;
using ApiService.Common.Services;
using Carter;
using FluentValidation;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.EntityFrameworkCore;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Add Aspire service defaults (observability, health checks)
builder.AddServiceDefaults();

// Add Database with Aspire integration (automatic health checks + telemetry)
builder.AddNpgsqlDbContext<ApplicationDbContext>("defi-db", configureDbContextOptions: options =>
{
    options.UseNpgsql(npgsqlOptions =>
    {
        npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "dash");
    });
});

// Add services to the container
builder.Services.AddCarter();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add FluentValidation
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

// Add MediatR with pipeline behaviors
builder.Services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
});

// Add SignalR
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
    options.MaximumReceiveMessageSize = 102400;
});

// Register notification service
builder.Services.AddSingleton<INotificationService, SignalRNotificationService>();

// Register External Providers
builder.Services.AddHttpClient<IBlockchainDataProvider, MoralisProvider>(client =>
{
    var baseUrl = builder.Configuration["ExternalProviders:Moralis:BaseUrl"] ?? "https://deep-index.moralis.io/api/v2.2";
    client.BaseAddress = new Uri(baseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddHttpClient<IOpenFinanceProvider, PluggyProvider>(client =>
{
    var baseUrl = builder.Configuration["ExternalProviders:Pluggy:BaseUrl"] ?? "https://api.pluggy.ai";
    client.BaseAddress = new Uri(baseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Add Hangfire for background jobs
builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(options =>
        options.UseNpgsqlConnection(builder.Configuration.GetConnectionString("defi-db")!)));

builder.Services.AddHangfireServer(options =>
{
    options.WorkerCount = 5;
});

// Register background job services
builder.Services.AddScoped<WalletSyncJob>();
builder.Services.AddScoped<AccountSyncJob>();
builder.Services.AddScoped<PortfolioCalculationJob>();
builder.Services.AddScoped<AlertGenerationJob>();
builder.Services.AddScoped<ExportProcessingJob>();
builder.Services.AddScoped<ExportCleanupJob>();

// Register export services
builder.Services.AddScoped<IPdfExportService, PdfExportService>();
builder.Services.AddScoped<IExcelExportService, ExcelExportService>();

// Add Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
    {
        var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: ipAddress,
            factory: partition => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1)
            });
    });

    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        await context.HttpContext.Response.WriteAsJsonAsync(new
        {
            error = new
            {
                code = "RATE_LIMIT_EXCEEDED",
                message = "Too many requests. Please try again later."
            }
        }, cancellationToken);
    };
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "https://localhost:5173")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Run database migrations on startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.Migrate();
}

// Configure the HTTP request pipeline
app.MapDefaultEndpoints(); // Aspire health checks

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    // Hangfire dashboard (only in development)
    app.UseHangfireDashboard("/hangfire", new DashboardOptions
    {
        Authorization = Array.Empty<Hangfire.Dashboard.IDashboardAuthorizationFilter>()
    });
}

// Enable CORS
app.UseCors("AllowFrontend");

// Add Global Exception Middleware
app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseMiddleware<ValidationExceptionMiddleware>();

app.UseHttpsRedirection();

// Enable Rate Limiting
app.UseRateLimiter();

// Map Carter endpoints
app.MapCarter();

// Map SignalR hub
app.MapHub<DashboardHub>("/hubs/dashboard");

// Configure Hangfire recurring jobs
RecurringJob.AddOrUpdate<WalletSyncJob>("wallet-sync", job => job.ExecuteAsync(), "*/5 * * * *");
RecurringJob.AddOrUpdate<AccountSyncJob>("account-sync", job => job.ExecuteAsync(), "*/15 * * * *");
RecurringJob.AddOrUpdate<PortfolioCalculationJob>("portfolio-calculation", job => job.ExecuteAsync(), "0 * * * *");
RecurringJob.AddOrUpdate<AlertGenerationJob>("alert-generation", job => job.ExecuteAsync(), "*/30 * * * *");
RecurringJob.AddOrUpdate<ExportCleanupJob>("export-cleanup", job => job.ExecuteAsync(), "0 3 * * *");

app.Run();

// Make Program class accessible to integration tests
public partial class Program { }
