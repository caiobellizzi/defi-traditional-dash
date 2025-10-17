using Microsoft.AspNetCore.SignalR;

namespace ApiService.Common.Hubs;

/// <summary>
/// SignalR hub for real-time dashboard updates.
/// Clients can subscribe to specific resource updates (clients, wallets, accounts, alerts).
/// </summary>
public class DashboardHub : Hub
{
    private readonly ILogger<DashboardHub> _logger;

    public DashboardHub(ILogger<DashboardHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("Client connected: {ConnectionId}", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (exception != null)
        {
            _logger.LogWarning(exception, "Client disconnected with error: {ConnectionId}", Context.ConnectionId);
        }
        else
        {
            _logger.LogInformation("Client disconnected: {ConnectionId}", Context.ConnectionId);
        }
        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Subscribe to portfolio updates for a specific client.
    /// </summary>
    public async Task SubscribeToClient(Guid clientId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"client_{clientId}");
        _logger.LogInformation("Connection {ConnectionId} subscribed to client {ClientId}",
            Context.ConnectionId, clientId);
    }

    /// <summary>
    /// Unsubscribe from client portfolio updates.
    /// </summary>
    public async Task UnsubscribeFromClient(Guid clientId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"client_{clientId}");
        _logger.LogInformation("Connection {ConnectionId} unsubscribed from client {ClientId}",
            Context.ConnectionId, clientId);
    }

    /// <summary>
    /// Subscribe to balance updates for a specific wallet.
    /// </summary>
    public async Task SubscribeToWallet(Guid walletId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"wallet_{walletId}");
        _logger.LogInformation("Connection {ConnectionId} subscribed to wallet {WalletId}",
            Context.ConnectionId, walletId);
    }

    /// <summary>
    /// Unsubscribe from wallet balance updates.
    /// </summary>
    public async Task UnsubscribeFromWallet(Guid walletId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"wallet_{walletId}");
        _logger.LogInformation("Connection {ConnectionId} unsubscribed from wallet {WalletId}",
            Context.ConnectionId, walletId);
    }

    /// <summary>
    /// Subscribe to balance updates for a specific account.
    /// </summary>
    public async Task SubscribeToAccount(Guid accountId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"account_{accountId}");
        _logger.LogInformation("Connection {ConnectionId} subscribed to account {AccountId}",
            Context.ConnectionId, accountId);
    }

    /// <summary>
    /// Unsubscribe from account balance updates.
    /// </summary>
    public async Task UnsubscribeFromAccount(Guid accountId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"account_{accountId}");
        _logger.LogInformation("Connection {ConnectionId} unsubscribed from account {AccountId}",
            Context.ConnectionId, accountId);
    }

    /// <summary>
    /// Subscribe to all system alerts.
    /// </summary>
    public async Task SubscribeToAlerts()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "alerts");
        _logger.LogInformation("Connection {ConnectionId} subscribed to alerts", Context.ConnectionId);
    }

    /// <summary>
    /// Unsubscribe from system alerts.
    /// </summary>
    public async Task UnsubscribeFromAlerts()
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "alerts");
        _logger.LogInformation("Connection {ConnectionId} unsubscribed from alerts", Context.ConnectionId);
    }

    /// <summary>
    /// Subscribe to global dashboard updates (aggregated metrics, system status).
    /// </summary>
    public async Task SubscribeToDashboard()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "dashboard");
        _logger.LogInformation("Connection {ConnectionId} subscribed to dashboard", Context.ConnectionId);
    }

    /// <summary>
    /// Unsubscribe from global dashboard updates.
    /// </summary>
    public async Task UnsubscribeFromDashboard()
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "dashboard");
        _logger.LogInformation("Connection {ConnectionId} unsubscribed from dashboard", Context.ConnectionId);
    }
}
