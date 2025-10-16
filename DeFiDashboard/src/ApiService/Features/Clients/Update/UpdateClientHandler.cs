using ApiService.Common.Database;
using ApiService.Common.Utilities;
using ApiService.Features.Clients.Create;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ApiService.Features.Clients.Update;

public class UpdateClientHandler : IRequestHandler<UpdateClientCommand, Result<bool>>
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<UpdateClientHandler> _logger;

    public UpdateClientHandler(ApplicationDbContext context, ILogger<UpdateClientHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(
        UpdateClientCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var client = await _context.Clients
                .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

            if (client == null)
            {
                return Result<bool>.Failure($"Client with ID {request.Id} not found");
            }

            // Check if email is being changed and already exists
            if (client.Email != request.Email)
            {
                var emailExists = await _context.Clients
                    .AnyAsync(c => c.Email == request.Email && c.Id != request.Id, cancellationToken);

                if (emailExists)
                {
                    return Result<bool>.Failure("A client with this email already exists");
                }
            }

            // Check if document is being changed and already exists
            if (!string.IsNullOrEmpty(request.Document) && client.Document != request.Document)
            {
                var documentExists = await _context.Clients
                    .AnyAsync(c => c.Document == request.Document && c.Id != request.Id, cancellationToken);

                if (documentExists)
                {
                    return Result<bool>.Failure("A client with this document already exists");
                }
            }

            // Update client
            client.Name = request.Name;
            client.Email = request.Email;
            client.Document = request.Document;
            client.PhoneNumber = request.PhoneNumber;
            client.Notes = InputSanitizer.Sanitize(request.Notes);
            if (request.Status != null)
            {
                client.Status = request.Status;
            }
            client.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Updated client {ClientId}", client.Id);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating client {ClientId}", request.Id);
            return Result<bool>.Failure("An error occurred while updating the client");
        }
    }
}
