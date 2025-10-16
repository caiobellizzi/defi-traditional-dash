using ApiService.Common.Behaviors;
using ApiService.Common.Database;
using ApiService.Common.Middleware;
using Carter;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add Aspire service defaults (observability, health checks)
builder.AddServiceDefaults();

// Add Database with Aspire integration (automatic health checks + telemetry)
builder.AddNpgsqlDbContext<ApplicationDbContext>("defi-db");

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

// Configure the HTTP request pipeline
app.MapDefaultEndpoints(); // Aspire health checks

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Enable CORS
app.UseCors("AllowFrontend");

app.UseMiddleware<ValidationExceptionMiddleware>();

app.UseHttpsRedirection();

// Map Carter endpoints
app.MapCarter();

app.Run();

// Make Program class accessible to integration tests
public partial class Program { }
