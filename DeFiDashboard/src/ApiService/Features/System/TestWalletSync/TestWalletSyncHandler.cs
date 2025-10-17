using ApiService.BackgroundJobs;
using ApiService.Features.Alerts.GetList;
using MediatR;

namespace ApiService.Features.System.TestWalletSync;

public class TestWalletSyncHandler : IRequestHandler<TestWalletSyncCommand, Result<TestWalletSyncResultDto>>
{
    private readonly WalletSyncJob _walletSyncJob;
    private readonly ILogger<TestWalletSyncHandler> _logger;

    public TestWalletSyncHandler(
        WalletSyncJob walletSyncJob,
        ILogger<TestWalletSyncHandler> logger)
    {
        _walletSyncJob = walletSyncJob;
        _logger = logger;
    }

    public async Task<Result<TestWalletSyncResultDto>> Handle(
        TestWalletSyncCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var jobId = Guid.NewGuid().ToString();
            _logger.LogInformation("Test wallet sync started: {JobId}", jobId);

            var startedAt = DateTime.UtcNow;

            // Execute the wallet sync job synchronously for testing
            await _walletSyncJob.ExecuteAsync();

            var result = new TestWalletSyncResultDto
            {
                JobId = jobId,
                Status = "Completed",
                StartedAt = startedAt,
                WalletsProcessed = 0, // Would need to track this in the job
                TotalBalancesUpdated = 0,
                TotalTransactionsAdded = 0,
                Errors = new List<string>()
            };

            _logger.LogInformation("Test wallet sync completed: {JobId}", jobId);

            return Result<TestWalletSyncResultDto>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during test wallet sync");
            return Result<TestWalletSyncResultDto>.Failure($"Sync failed: {ex.Message}");
        }
    }
}
