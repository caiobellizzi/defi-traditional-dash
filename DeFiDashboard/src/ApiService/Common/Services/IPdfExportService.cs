namespace ApiService.Common.Services;

public interface IPdfExportService
{
    Task<byte[]> GeneratePortfolioReportAsync(Guid clientId, bool includeTransactions, CancellationToken ct = default);
    Task<byte[]> GeneratePerformanceReportAsync(Guid clientId, DateTime fromDate, DateTime toDate, CancellationToken ct = default);
}
