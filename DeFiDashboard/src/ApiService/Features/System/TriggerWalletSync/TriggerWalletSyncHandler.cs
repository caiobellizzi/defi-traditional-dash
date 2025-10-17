using ApiService.Features.Alerts.GetList;
using MediatR;

namespace ApiService.Features.System.TriggerWalletSync;

public class TriggerWalletSyncHandler : IRequestHandler<TriggerWalletSyncCommand, Result<SyncJobDto>>
{
    private readonly ILogger<TriggerWalletSyncHandler> _logger;

    public TriggerWalletSyncHandler(ILogger<TriggerWalletSyncHandler> logger)
    {
        _logger = logger;
    }

    public async Task<Result<SyncJobDto>> Handle(
        TriggerWalletSyncCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Placeholder implementation - return mock job data
            // In a real system, this would enqueue a Hangfire job or trigger a background service
            var jobId = Guid.NewGuid();

            _logger.LogInformation("Manual wallet sync triggered: {JobId}", jobId);

            var job = new SyncJobDto
            {
                JobId = jobId,
                JobType = "WalletSync",
                Status = "queued",
                TriggeredAt = DateTime.UtcNow
            };

            return await Task.FromResult(Result<SyncJobDto>.Success(job));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error triggering wallet sync");
            return Result<SyncJobDto>.Failure("An error occurred while triggering wallet sync");
        }
    }
}
