using ApiService.Common.Database;
using ApiService.Common.Database.Entities;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;

namespace ApiService.Common.Services;

public class ExcelExportService : IExcelExportService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ExcelExportService> _logger;

    public ExcelExportService(ApplicationDbContext context, ILogger<ExcelExportService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<byte[]> GenerateTransactionsExportAsync(
        DateTime? fromDate,
        DateTime? toDate,
        Guid? clientId,
        string? transactionType,
        CancellationToken ct = default)
    {
        _logger.LogInformation("Generating transactions export with filters: fromDate={FromDate}, toDate={ToDate}, clientId={ClientId}, type={Type}",
            fromDate, toDate, clientId, transactionType);

        var query = _context.Transactions.AsNoTracking().AsQueryable();

        if (fromDate.HasValue)
            query = query.Where(t => t.TransactionDate >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(t => t.TransactionDate <= toDate.Value);

        if (!string.IsNullOrWhiteSpace(transactionType))
            query = query.Where(t => t.TransactionType == transactionType);

        // If clientId is provided, filter by allocations
        if (clientId.HasValue)
        {
            var allocations = await _context.ClientAssetAllocations
                .AsNoTracking()
                .Where(a => a.ClientId == clientId.Value && a.EndDate == null)
                .Select(a => a.AssetId)
                .ToListAsync(ct);

            query = query.Where(t => allocations.Contains(t.AssetId));
        }

        var transactions = await query.OrderByDescending(t => t.TransactionDate).ToListAsync(ct);

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Transactions");

        // Add filter info
        worksheet.Cell(1, 1).Value = "Transactions Export";
        worksheet.Cell(1, 1).Style.Font.Bold = true;
        worksheet.Cell(1, 1).Style.Font.FontSize = 14;

        int currentRow = 2;
        worksheet.Cell(currentRow++, 1).Value = $"Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC";
        if (fromDate.HasValue)
            worksheet.Cell(currentRow++, 1).Value = $"From Date: {fromDate.Value:yyyy-MM-dd}";
        if (toDate.HasValue)
            worksheet.Cell(currentRow++, 1).Value = $"To Date: {toDate.Value:yyyy-MM-dd}";
        if (!string.IsNullOrWhiteSpace(transactionType))
            worksheet.Cell(currentRow++, 1).Value = $"Type: {transactionType}";

        currentRow++; // Empty row

        // Add headers
        int headerRow = currentRow++;
        worksheet.Cell(headerRow, 1).Value = "Date";
        worksheet.Cell(headerRow, 2).Value = "Time";
        worksheet.Cell(headerRow, 3).Value = "Type";
        worksheet.Cell(headerRow, 4).Value = "Asset/Category";
        worksheet.Cell(headerRow, 5).Value = "Direction";
        worksheet.Cell(headerRow, 6).Value = "Amount";
        worksheet.Cell(headerRow, 7).Value = "Fee";
        worksheet.Cell(headerRow, 8).Value = "USD Value";
        worksheet.Cell(headerRow, 9).Value = "Status";
        worksheet.Cell(headerRow, 10).Value = "Source";
        worksheet.Cell(headerRow, 11).Value = "Hash/Reference";

        // Style headers
        var headerRange = worksheet.Range(headerRow, 1, headerRow, 11);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;
        headerRange.Style.Border.BottomBorder = XLBorderStyleValues.Medium;

        // Add data
        foreach (var transaction in transactions)
        {
            worksheet.Cell(currentRow, 1).Value = transaction.TransactionDate.ToString("yyyy-MM-dd");
            worksheet.Cell(currentRow, 2).Value = transaction.TransactionDate.ToString("HH:mm:ss");
            worksheet.Cell(currentRow, 3).Value = transaction.TransactionType;
            worksheet.Cell(currentRow, 4).Value = transaction.TokenSymbol ?? transaction.Category ?? "N/A";
            worksheet.Cell(currentRow, 5).Value = transaction.Direction ?? "N/A";
            worksheet.Cell(currentRow, 6).Value = transaction.Amount;
            worksheet.Cell(currentRow, 7).Value = transaction.Fee ?? 0;
            worksheet.Cell(currentRow, 8).Value = transaction.AmountUsd ?? 0;
            worksheet.Cell(currentRow, 9).Value = transaction.Status;
            worksheet.Cell(currentRow, 10).Value = transaction.IsManualEntry ? "Manual" : "Auto";
            worksheet.Cell(currentRow, 11).Value = transaction.TransactionHash ?? transaction.ExternalId ?? "";

            // Format numbers
            worksheet.Cell(currentRow, 6).Style.NumberFormat.Format = "#,##0.00000000";
            worksheet.Cell(currentRow, 7).Style.NumberFormat.Format = "#,##0.00000000";
            worksheet.Cell(currentRow, 8).Style.NumberFormat.Format = "$#,##0.00";

            currentRow++;
        }

        // Create table
        if (transactions.Any())
        {
            var dataRange = worksheet.Range(headerRow, 1, currentRow - 1, 11);
            var table = dataRange.CreateTable();
            table.Theme = XLTableTheme.TableStyleMedium2;
        }

        // Add summary
        currentRow++;
        worksheet.Cell(currentRow, 1).Value = "Summary";
        worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
        currentRow++;
        worksheet.Cell(currentRow, 1).Value = "Total Transactions:";
        worksheet.Cell(currentRow, 2).Value = transactions.Count;
        currentRow++;
        worksheet.Cell(currentRow, 1).Value = "Total USD Value:";
        worksheet.Cell(currentRow, 2).Value = transactions.Sum(t => t.AmountUsd ?? 0);
        worksheet.Cell(currentRow, 2).Style.NumberFormat.Format = "$#,##0.00";

        // Auto-fit columns
        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public async Task<byte[]> GeneratePerformanceExportAsync(
        Guid? clientId,
        DateTime fromDate,
        DateTime toDate,
        CancellationToken ct = default)
    {
        _logger.LogInformation("Generating performance export for clientId={ClientId} from {FromDate} to {ToDate}",
            clientId, fromDate, toDate);

        using var workbook = new XLWorkbook();

        // Summary Sheet
        var summarySheet = workbook.Worksheets.Add("Summary");
        var metricsSheet = workbook.Worksheets.Add("Daily Performance");

        // Fetch performance metrics
        var query = _context.PerformanceMetrics.AsNoTracking().AsQueryable();

        if (clientId.HasValue)
            query = query.Where(m => m.ClientId == clientId.Value);

        query = query.Where(m => m.CalculatedAt >= fromDate && m.CalculatedAt <= toDate);

        var metrics = await query.OrderBy(m => m.CalculatedAt).ToListAsync(ct);

        // Summary Sheet
        summarySheet.Cell(1, 1).Value = "Performance Summary";
        summarySheet.Cell(1, 1).Style.Font.Bold = true;
        summarySheet.Cell(1, 1).Style.Font.FontSize = 14;

        int summaryRow = 3;
        summarySheet.Cell(summaryRow++, 1).Value = $"Period: {fromDate:yyyy-MM-dd} to {toDate:yyyy-MM-dd}";
        summarySheet.Cell(summaryRow++, 1).Value = $"Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC";

        summaryRow++;

        if (metrics.Any())
        {
            var firstMetric = metrics.First();
            var lastMetric = metrics.Last();
            var totalReturn = firstMetric.TotalValueUsd > 0
                ? ((lastMetric.TotalValueUsd - firstMetric.TotalValueUsd) / firstMetric.TotalValueUsd * 100)
                : 0;

            summarySheet.Cell(summaryRow, 1).Value = "Metric";
            summarySheet.Cell(summaryRow, 2).Value = "Value";
            summarySheet.Range(summaryRow, 1, summaryRow, 2).Style.Font.Bold = true;
            summarySheet.Range(summaryRow, 1, summaryRow, 2).Style.Fill.BackgroundColor = XLColor.LightBlue;
            summaryRow++;

            summarySheet.Cell(summaryRow, 1).Value = "Starting Value";
            summarySheet.Cell(summaryRow, 2).Value = firstMetric.TotalValueUsd;
            summarySheet.Cell(summaryRow, 2).Style.NumberFormat.Format = "$#,##0.00";
            summaryRow++;

            summarySheet.Cell(summaryRow, 1).Value = "Ending Value";
            summarySheet.Cell(summaryRow, 2).Value = lastMetric.TotalValueUsd;
            summarySheet.Cell(summaryRow, 2).Style.NumberFormat.Format = "$#,##0.00";
            summaryRow++;

            summarySheet.Cell(summaryRow, 1).Value = "Total Return %";
            summarySheet.Cell(summaryRow, 2).Value = totalReturn / 100;
            summarySheet.Cell(summaryRow, 2).Style.NumberFormat.Format = "0.00%";
            summaryRow++;

            summarySheet.Cell(summaryRow, 1).Value = "Total P&L";
            summarySheet.Cell(summaryRow, 2).Value = lastMetric.ProfitLoss ?? 0;
            summarySheet.Cell(summaryRow, 2).Style.NumberFormat.Format = "$#,##0.00";
            summaryRow++;

            summarySheet.Cell(summaryRow, 1).Value = "Average ROI %";
            summarySheet.Cell(summaryRow, 2).Value = (metrics.Average(m => m.Roi ?? 0)) / 100;
            summarySheet.Cell(summaryRow, 2).Style.NumberFormat.Format = "0.00%";
        }
        else
        {
            summarySheet.Cell(summaryRow, 1).Value = "No data available for the selected period.";
        }

        summarySheet.Columns().AdjustToContents();

        // Daily Performance Sheet
        metricsSheet.Cell(1, 1).Value = "Date";
        metricsSheet.Cell(1, 2).Value = "Total Value";
        metricsSheet.Cell(1, 3).Value = "Crypto Value";
        metricsSheet.Cell(1, 4).Value = "Traditional Value";
        metricsSheet.Cell(1, 5).Value = "ROI %";
        metricsSheet.Cell(1, 6).Value = "Realized P&L";
        metricsSheet.Cell(1, 7).Value = "Unrealized P&L";

        var metricsHeaderRange = metricsSheet.Range(1, 1, 1, 7);
        metricsHeaderRange.Style.Font.Bold = true;
        metricsHeaderRange.Style.Fill.BackgroundColor = XLColor.LightBlue;
        metricsHeaderRange.Style.Border.BottomBorder = XLBorderStyleValues.Medium;

        int metricsRow = 2;
        foreach (var metric in metrics)
        {
            metricsSheet.Cell(metricsRow, 1).Value = metric.CalculationDate.ToString("yyyy-MM-dd");
            metricsSheet.Cell(metricsRow, 2).Value = metric.TotalValueUsd;
            metricsSheet.Cell(metricsRow, 3).Value = metric.CryptoValueUsd ?? 0;
            metricsSheet.Cell(metricsRow, 4).Value = metric.TraditionalValueUsd ?? 0;
            metricsSheet.Cell(metricsRow, 5).Value = (metric.Roi ?? 0) / 100;
            metricsSheet.Cell(metricsRow, 6).Value = metric.ProfitLoss ?? 0;
            metricsSheet.Cell(metricsRow, 7).Value = 0; // Unrealized P&L not tracked in entity

            metricsSheet.Cell(metricsRow, 2).Style.NumberFormat.Format = "$#,##0.00";
            metricsSheet.Cell(metricsRow, 3).Style.NumberFormat.Format = "$#,##0.00";
            metricsSheet.Cell(metricsRow, 4).Style.NumberFormat.Format = "$#,##0.00";
            metricsSheet.Cell(metricsRow, 5).Style.NumberFormat.Format = "0.00%";
            metricsSheet.Cell(metricsRow, 6).Style.NumberFormat.Format = "$#,##0.00";
            metricsSheet.Cell(metricsRow, 7).Style.NumberFormat.Format = "$#,##0.00";

            metricsRow++;
        }

        if (metrics.Any())
        {
            var metricsDataRange = metricsSheet.Range(1, 1, metricsRow - 1, 7);
            var metricsTable = metricsDataRange.CreateTable();
            metricsTable.Theme = XLTableTheme.TableStyleMedium2;
        }

        metricsSheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public async Task<byte[]> GenerateAllocationsExportAsync(
        Guid? clientId,
        bool activeOnly,
        CancellationToken ct = default)
    {
        _logger.LogInformation("Generating allocations export for clientId={ClientId}, activeOnly={ActiveOnly}",
            clientId, activeOnly);

        var query = _context.ClientAssetAllocations
            .Include(a => a.Client)
            .AsNoTracking()
            .AsQueryable();

        if (clientId.HasValue)
            query = query.Where(a => a.ClientId == clientId.Value);

        if (activeOnly)
            query = query.Where(a => a.EndDate == null);

        var allocations = await query.OrderBy(a => a.Client.Name).ThenBy(a => a.StartDate).ToListAsync(ct);

        // Fetch wallet and account details
        var walletIds = allocations.Where(a => a.AssetType == "Wallet").Select(a => a.AssetId).Distinct().ToList();
        var accountIds = allocations.Where(a => a.AssetType == "Account").Select(a => a.AssetId).Distinct().ToList();

        var wallets = await _context.CustodyWallets
            .AsNoTracking()
            .Where(w => walletIds.Contains(w.Id))
            .ToDictionaryAsync(w => w.Id, ct);

        var accounts = await _context.TraditionalAccounts
            .AsNoTracking()
            .Where(a => accountIds.Contains(a.Id))
            .ToDictionaryAsync(a => a.Id, ct);

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Allocations");

        // Title
        worksheet.Cell(1, 1).Value = "Client Asset Allocations";
        worksheet.Cell(1, 1).Style.Font.Bold = true;
        worksheet.Cell(1, 1).Style.Font.FontSize = 14;

        int currentRow = 3;
        worksheet.Cell(currentRow++, 1).Value = $"Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC";
        worksheet.Cell(currentRow++, 1).Value = $"Active Only: {activeOnly}";

        currentRow++; // Empty row

        // Headers
        int headerRow = currentRow++;
        worksheet.Cell(headerRow, 1).Value = "Client";
        worksheet.Cell(headerRow, 2).Value = "Asset Type";
        worksheet.Cell(headerRow, 3).Value = "Asset Identifier";
        worksheet.Cell(headerRow, 4).Value = "Allocation Type";
        worksheet.Cell(headerRow, 5).Value = "Allocation Value";
        worksheet.Cell(headerRow, 6).Value = "Start Date";
        worksheet.Cell(headerRow, 7).Value = "End Date";
        worksheet.Cell(headerRow, 8).Value = "Status";
        worksheet.Cell(headerRow, 9).Value = "Notes";

        var headerRange = worksheet.Range(headerRow, 1, headerRow, 9);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;
        headerRange.Style.Border.BottomBorder = XLBorderStyleValues.Medium;

        // Data
        foreach (var allocation in allocations)
        {
            worksheet.Cell(currentRow, 1).Value = allocation.Client.Name;
            worksheet.Cell(currentRow, 2).Value = allocation.AssetType;

            // Asset identifier
            string assetIdentifier = "Unknown";
            if (allocation.AssetType == "Wallet" && wallets.TryGetValue(allocation.AssetId, out var wallet))
            {
                var chain = wallet.SupportedChains?.FirstOrDefault() ?? "Unknown";
                assetIdentifier = $"{wallet.WalletAddress[..8]}...{wallet.WalletAddress[^6..]} ({chain})";
            }
            else if (allocation.AssetType == "Account" && accounts.TryGetValue(allocation.AssetId, out var account))
            {
                var accountName = account.Label ?? account.InstitutionName ?? "Unknown";
                assetIdentifier = $"{accountName} ({account.AccountType ?? "N/A"})";
            }
            worksheet.Cell(currentRow, 3).Value = assetIdentifier;

            worksheet.Cell(currentRow, 4).Value = allocation.AllocationType;
            worksheet.Cell(currentRow, 5).Value = allocation.AllocationValue;
            worksheet.Cell(currentRow, 6).Value = allocation.StartDate.ToString("yyyy-MM-dd");
            worksheet.Cell(currentRow, 7).Value = allocation.EndDate?.ToString("yyyy-MM-dd") ?? "Active";
            worksheet.Cell(currentRow, 8).Value = allocation.EndDate == null ? "Active" : "Ended";
            worksheet.Cell(currentRow, 9).Value = allocation.Notes ?? "";

            // Format allocation value
            if (allocation.AllocationType == "Percentage")
            {
                worksheet.Cell(currentRow, 5).Style.NumberFormat.Format = "0.00\"%\"";
            }
            else
            {
                worksheet.Cell(currentRow, 5).Style.NumberFormat.Format = "#,##0.00";
            }

            // Color code status
            if (allocation.EndDate == null)
            {
                worksheet.Cell(currentRow, 8).Style.Fill.BackgroundColor = XLColor.LightGreen;
            }
            else
            {
                worksheet.Cell(currentRow, 8).Style.Fill.BackgroundColor = XLColor.LightGray;
            }

            currentRow++;
        }

        // Create table
        if (allocations.Any())
        {
            var dataRange = worksheet.Range(headerRow, 1, currentRow - 1, 9);
            var table = dataRange.CreateTable();
            table.Theme = XLTableTheme.TableStyleMedium2;
        }

        // Summary
        currentRow++;
        worksheet.Cell(currentRow, 1).Value = "Summary";
        worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
        currentRow++;
        worksheet.Cell(currentRow, 1).Value = "Total Allocations:";
        worksheet.Cell(currentRow, 2).Value = allocations.Count;
        currentRow++;
        worksheet.Cell(currentRow, 1).Value = "Active Allocations:";
        worksheet.Cell(currentRow, 2).Value = allocations.Count(a => a.EndDate == null);
        currentRow++;
        worksheet.Cell(currentRow, 1).Value = "Ended Allocations:";
        worksheet.Cell(currentRow, 2).Value = allocations.Count(a => a.EndDate != null);

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}
