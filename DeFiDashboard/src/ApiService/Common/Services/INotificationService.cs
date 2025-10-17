namespace ApiService.Common.Services;

/// <summary>
/// Service for sending real-time notifications to connected clients via SignalR.
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Notify clients subscribed to a specific client's portfolio when it's updated.
    /// </summary>
    /// <param name="clientId">Client ID</param>
    /// <param name="portfolioData">Updated portfolio data</param>
    Task NotifyPortfolioUpdateAsync(Guid clientId, object portfolioData);

    /// <summary>
    /// Notify clients subscribed to a specific wallet when its balance is updated.
    /// </summary>
    /// <param name="walletId">Wallet ID</param>
    /// <param name="balanceData">Updated balance data</param>
    Task NotifyWalletBalanceUpdateAsync(Guid walletId, object balanceData);

    /// <summary>
    /// Notify clients subscribed to a specific account when its balance is updated.
    /// </summary>
    /// <param name="accountId">Account ID</param>
    /// <param name="balanceData">Updated balance data</param>
    Task NotifyAccountBalanceUpdateAsync(Guid accountId, object balanceData);

    /// <summary>
    /// Notify clients when a new transaction is detected for a wallet or account.
    /// </summary>
    /// <param name="assetId">Wallet or Account ID</param>
    /// <param name="assetType">Either "Wallet" or "Account"</param>
    /// <param name="transactionData">Transaction details</param>
    Task NotifyNewTransactionAsync(Guid assetId, string assetType, object transactionData);

    /// <summary>
    /// Notify all clients subscribed to alerts when a new alert is generated.
    /// </summary>
    /// <param name="alert">Alert data</param>
    Task NotifyNewAlertAsync(object alert);

    /// <summary>
    /// Notify a specific client when their allocation has drifted beyond thresholds.
    /// </summary>
    /// <param name="clientId">Client ID</param>
    /// <param name="driftData">Allocation drift details</param>
    Task NotifyAllocationDriftAsync(Guid clientId, object driftData);

    /// <summary>
    /// Broadcast a system-wide message to all connected clients.
    /// </summary>
    /// <param name="message">Message content</param>
    /// <param name="severity">Severity level: "info", "warning", "error", "success"</param>
    Task BroadcastSystemMessageAsync(string message, string severity = "info");

    /// <summary>
    /// Notify dashboard subscribers when global metrics are updated.
    /// </summary>
    /// <param name="metricsData">Updated dashboard metrics</param>
    Task NotifyDashboardMetricsUpdateAsync(object metricsData);

    /// <summary>
    /// Notify when an alert status changes (acknowledged/resolved).
    /// </summary>
    /// <param name="alertId">Alert ID</param>
    /// <param name="newStatus">New alert status</param>
    /// <param name="alertData">Updated alert data</param>
    Task NotifyAlertStatusChangedAsync(Guid alertId, string newStatus, object alertData);
}
