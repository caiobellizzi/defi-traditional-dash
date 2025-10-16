using Microsoft.AspNetCore.SignalR;
using ApiService.Common.Hubs;

namespace ApiService.Common.Services;

/// <summary>
/// SignalR implementation of the notification service.
/// Sends real-time updates to connected clients via the DashboardHub.
/// </summary>
public class SignalRNotificationService : INotificationService
{
    private readonly IHubContext<DashboardHub> _hubContext;
    private readonly ILogger<SignalRNotificationService> _logger;

    public SignalRNotificationService(
        IHubContext<DashboardHub> hubContext,
        ILogger<SignalRNotificationService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task NotifyPortfolioUpdateAsync(Guid clientId, object portfolioData)
    {
        try
        {
            _logger.LogInformation("Sending portfolio update for client {ClientId}", clientId);
            await _hubContext.Clients
                .Group($"client_{clientId}")
                .SendAsync("PortfolioUpdated", new
                {
                    clientId,
                    timestamp = DateTime.UtcNow,
                    data = portfolioData
                });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send portfolio update for client {ClientId}", clientId);
        }
    }

    public async Task NotifyWalletBalanceUpdateAsync(Guid walletId, object balanceData)
    {
        try
        {
            _logger.LogInformation("Sending wallet balance update for wallet {WalletId}", walletId);
            await _hubContext.Clients
                .Group($"wallet_{walletId}")
                .SendAsync("WalletBalanceUpdated", new
                {
                    walletId,
                    timestamp = DateTime.UtcNow,
                    data = balanceData
                });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send wallet balance update for wallet {WalletId}", walletId);
        }
    }

    public async Task NotifyAccountBalanceUpdateAsync(Guid accountId, object balanceData)
    {
        try
        {
            _logger.LogInformation("Sending account balance update for account {AccountId}", accountId);
            await _hubContext.Clients
                .Group($"account_{accountId}")
                .SendAsync("AccountBalanceUpdated", new
                {
                    accountId,
                    timestamp = DateTime.UtcNow,
                    data = balanceData
                });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send account balance update for account {AccountId}", accountId);
        }
    }

    public async Task NotifyNewTransactionAsync(Guid assetId, string assetType, object transactionData)
    {
        try
        {
            _logger.LogInformation("Sending new transaction notification for {AssetType} {AssetId}",
                assetType, assetId);

            var groupName = assetType.ToLower() == "wallet"
                ? $"wallet_{assetId}"
                : $"account_{assetId}";

            await _hubContext.Clients
                .Group(groupName)
                .SendAsync("NewTransaction", new
                {
                    assetId,
                    assetType,
                    timestamp = DateTime.UtcNow,
                    data = transactionData
                });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send new transaction notification for {AssetType} {AssetId}",
                assetType, assetId);
        }
    }

    public async Task NotifyNewAlertAsync(object alert)
    {
        try
        {
            _logger.LogInformation("Broadcasting new alert to all subscribers");
            await _hubContext.Clients
                .Group("alerts")
                .SendAsync("NewAlert", new
                {
                    timestamp = DateTime.UtcNow,
                    data = alert
                });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to broadcast new alert");
        }
    }

    public async Task NotifyAllocationDriftAsync(Guid clientId, object driftData)
    {
        try
        {
            _logger.LogInformation("Sending allocation drift notification for client {ClientId}", clientId);
            await _hubContext.Clients
                .Group($"client_{clientId}")
                .SendAsync("AllocationDrift", new
                {
                    clientId,
                    timestamp = DateTime.UtcNow,
                    data = driftData
                });

            // Also send to alerts group
            await _hubContext.Clients
                .Group("alerts")
                .SendAsync("AllocationDrift", new
                {
                    clientId,
                    timestamp = DateTime.UtcNow,
                    data = driftData
                });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send allocation drift notification for client {ClientId}", clientId);
        }
    }

    public async Task BroadcastSystemMessageAsync(string message, string severity = "info")
    {
        try
        {
            _logger.LogInformation("Broadcasting system message with severity {Severity}: {Message}",
                severity, message);
            await _hubContext.Clients
                .All
                .SendAsync("SystemMessage", new
                {
                    message,
                    severity,
                    timestamp = DateTime.UtcNow
                });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to broadcast system message");
        }
    }

    public async Task NotifyDashboardMetricsUpdateAsync(object metricsData)
    {
        try
        {
            _logger.LogInformation("Sending dashboard metrics update");
            await _hubContext.Clients
                .Group("dashboard")
                .SendAsync("DashboardMetricsUpdated", new
                {
                    timestamp = DateTime.UtcNow,
                    data = metricsData
                });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send dashboard metrics update");
        }
    }

    public async Task NotifyAlertStatusChangedAsync(Guid alertId, string newStatus, object alertData)
    {
        try
        {
            _logger.LogInformation("Sending alert status change notification for alert {AlertId}: {Status}",
                alertId, newStatus);
            await _hubContext.Clients
                .Group("alerts")
                .SendAsync("AlertStatusChanged", new
                {
                    alertId,
                    newStatus,
                    timestamp = DateTime.UtcNow,
                    data = alertData
                });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send alert status change notification for alert {AlertId}", alertId);
        }
    }
}
