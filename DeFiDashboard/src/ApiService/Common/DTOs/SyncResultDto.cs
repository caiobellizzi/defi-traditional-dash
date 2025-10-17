namespace ApiService.Common.DTOs;

public record SyncResultDto
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public DateTime SyncedAt { get; init; }
    public int BalancesUpdated { get; init; }
    public int TransactionsAdded { get; init; }
}
