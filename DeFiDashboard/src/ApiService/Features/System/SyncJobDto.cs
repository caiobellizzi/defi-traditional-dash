namespace ApiService.Features.System;

public record SyncJobDto
{
    public Guid JobId { get; init; }
    public string JobType { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTime TriggeredAt { get; init; }
}
