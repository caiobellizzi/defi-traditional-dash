using ApiService.Common.Database;
using ApiService.Features.Clients.Create;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ApiService.Features.Clients.GetList;

public class GetClientsHandler : IRequestHandler<GetClientsQuery, Result<PagedResult<ClientDto>>>
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<GetClientsHandler> _logger;

    public GetClientsHandler(ApplicationDbContext context, ILogger<GetClientsHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<PagedResult<ClientDto>>> Handle(
        GetClientsQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = _context.Clients.AsNoTracking();

            // Filter by status if provided
            if (!string.IsNullOrEmpty(request.Status))
            {
                query = query.Where(c => c.Status == request.Status);
            }

            // Get total count
            var totalCount = await query.CountAsync(cancellationToken);

            // Apply pagination
            var clients = await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
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
                .ToListAsync(cancellationToken);

            var result = new PagedResult<ClientDto>
            {
                Items = clients,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };

            return Result<PagedResult<ClientDto>>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving clients list");
            return Result<PagedResult<ClientDto>>.Failure("An error occurred while retrieving clients");
        }
    }
}
