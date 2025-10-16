using ApiService.Common.Database;
using ApiService.Common.Database.Entities;
using ApiService.Common.Utilities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ApiService.Features.Clients.Create;

public class CreateClientHandler : IRequestHandler<CreateClientCommand, Result<Guid>>
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<CreateClientHandler> _logger;

    public CreateClientHandler(ApplicationDbContext context, ILogger<CreateClientHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(CreateClientCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Check if email already exists
            var emailExists = await _context.Clients
                .AnyAsync(c => c.Email == request.Email, cancellationToken);

            if (emailExists)
            {
                return Result<Guid>.Failure("A client with this email already exists");
            }

            // Check if document already exists (if provided)
            if (!string.IsNullOrEmpty(request.Document))
            {
                var documentExists = await _context.Clients
                    .AnyAsync(c => c.Document == request.Document, cancellationToken);

                if (documentExists)
                {
                    return Result<Guid>.Failure("A client with this document already exists");
                }
            }

            var client = new Client
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Email = request.Email,
                Document = request.Document,
                PhoneNumber = request.PhoneNumber,
                Notes = InputSanitizer.Sanitize(request.Notes),
                Status = "Active",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Clients.Add(client);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Created client {ClientId} with email {Email}", client.Id, client.Email);

            return Result<Guid>.Success(client.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating client with email {Email}", request.Email);
            return Result<Guid>.Failure("An error occurred while creating the client");
        }
    }
}
