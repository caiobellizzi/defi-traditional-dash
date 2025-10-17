namespace ApiService.Common.Services;

public interface IExcelExportService
{
    Task<byte[]> GenerateTransactionsExportAsync(DateTime? fromDate, DateTime? toDate, Guid? clientId, string? transactionType, CancellationToken ct = default);
    Task<byte[]> GeneratePerformanceExportAsync(Guid? clientId, DateTime fromDate, DateTime toDate, CancellationToken ct = default);
    Task<byte[]> GenerateAllocationsExportAsync(Guid? clientId, bool activeOnly, CancellationToken ct = default);
}
