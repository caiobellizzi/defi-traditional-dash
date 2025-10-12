using ApiService.Common.Database;
using ApiService.Features.Clients.Create;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ApiService.Features.Clients.Delete;

public class DeleteClientHandler : IRequestHandler<DeleteClientCommand, Result<bool>>
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DeleteClientHandler> _logger;

    public DeleteClientHandler(ApplicationDbContext context, ILogger<DeleteClientHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(
        DeleteClientCommand request,
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

            // Check if client has any allocations
            var hasAllocations = await _context.ClientAssetAllocations
                .AnyAsync(a => a.ClientId == request.Id && a.EndDate == null, cancellationToken);

            if (hasAllocations)
            {
                return Result<bool>.Failure("Cannot delete client with active asset allocations. Please end all allocations first.");
            }

            _context.Clients.Remove(client);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Deleted client {ClientId}", request.Id);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting client {ClientId}", request.Id);
            return Result<bool>.Failure("An error occurred while deleting the client");
        }
    }
}
