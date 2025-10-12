using ApiService.Common.Database;
using ApiService.Features.Clients.Create;
using ApiService.Features.Clients.GetList;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ApiService.Features.Clients.GetById;

public class GetClientByIdHandler : IRequestHandler<GetClientByIdQuery, Result<ClientDto>>
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<GetClientByIdHandler> _logger;

    public GetClientByIdHandler(ApplicationDbContext context, ILogger<GetClientByIdHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<ClientDto>> Handle(
        GetClientByIdQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var client = await _context.Clients
                .Where(c => c.Id == request.Id)
                .Select(c => new ClientDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Email = c.Email,
                    Document = c.Document,
                    PhoneNumber = c.PhoneNumber,
                    Status = c.Status,
                    Notes = c.Notes,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (client == null)
            {
                return Result<ClientDto>.Failure($"Client with ID {request.Id} not found");
            }

            return Result<ClientDto>.Success(client);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving client {ClientId}", request.Id);
            return Result<ClientDto>.Failure("An error occurred while retrieving the client");
        }
    }
}
