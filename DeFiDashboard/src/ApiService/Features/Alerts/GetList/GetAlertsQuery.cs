using MediatR;

namespace ApiService.Features.Alerts.GetList;

public record GetAlertsQuery(
    string? Severity = null,
    string? Status = null,
    string? AlertType = null,
    Guid? ClientId = null,
    int PageNumber = 1,
    int PageSize = 50
) : IRequest<Result<PagedResult<AlertDto>>>;

public record PagedResult<T>
{
    public IEnumerable<T> Items { get; init; } = [];
    public int TotalCount { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}

public record AlertDto
{
    public Guid Id { get; init; }
    public Guid? ClientId { get; init; }
    public string? ClientName { get; init; }
    public string AlertType { get; init; } = string.Empty;
    public string Severity { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public string? AlertData { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? AcknowledgedAt { get; init; }
    public DateTime? ResolvedAt { get; init; }
}

public record Result<T>
{
    public bool IsSuccess { get; init; }
    public T? Value { get; init; }
    public string? Error { get; init; }

    public static Result<T> Success(T value) => new() { IsSuccess = true, Value = value };
    public static Result<T> Failure(string error) => new() { IsSuccess = false, Error = error };
}
