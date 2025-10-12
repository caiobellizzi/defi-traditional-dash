using MediatR;

namespace ApiService.Features.Clients.Create;

public record CreateClientCommand(
    string Name,
    string Email,
    string? Document,
    string? PhoneNumber,
    string? Notes
) : IRequest<Result<Guid>>;

public record Result<T>
{
    public bool IsSuccess { get; init; }
    public T? Value { get; init; }
    public string? Error { get; init; }

    public static Result<T> Success(T value) => new() { IsSuccess = true, Value = value };
    public static Result<T> Failure(string error) => new() { IsSuccess = false, Error = error };
}
