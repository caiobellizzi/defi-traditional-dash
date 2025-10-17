using ApiService.Features.Alerts.GetList;
using ApiService.Features.System.TriggerWalletSync;
using MediatR;

namespace ApiService.Features.System.TriggerAccountSync;

public class TriggerAccountSyncHandler : IRequestHandler<TriggerAccountSyncCommand, Result<SyncJobDto>>
{
    private readonly ILogger<TriggerAccountSyncHandler> _logger;

    public TriggerAccountSyncHandler(ILogger<TriggerAccountSyncHandler> logger)
    {
        _logger = logger;
    }

    public async Task<Result<SyncJobDto>> Handle(
        TriggerAccountSyncCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Placeholder implementation - return mock job data
            // In a real system, this would enqueue a Hangfire job or trigger a background service
            var jobId = Guid.NewGuid();

            _logger.LogInformation("Manual account sync triggered: {JobId}", jobId);

            var job = new SyncJobDto
            {
                JobId = jobId,
                JobType = "AccountSync",
                Status = "queued",
                TriggeredAt = DateTime.UtcNow
            };

            return await Task.FromResult(Result<SyncJobDto>.Success(job));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error triggering account sync");
            return Result<SyncJobDto>.Failure("An error occurred while triggering account sync");
        }
    }
}
