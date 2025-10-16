using ApiService.Common.Database;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ApiService.Common.Services;

public class PdfExportService : IPdfExportService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<PdfExportService> _logger;

    public PdfExportService(ApplicationDbContext context, ILogger<PdfExportService> logger)
    {
        _context = context;
        _logger = logger;

        // Configure QuestPDF license (Community license for non-commercial use)
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<byte[]> GeneratePortfolioReportAsync(Guid clientId, bool includeTransactions, CancellationToken ct = default)
    {
        _logger.LogInformation("Generating portfolio report for client {ClientId}", clientId);

        // Fetch client data
        var client = await _context.Clients
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == clientId, ct);

        if (client == null)
        {
            throw new InvalidOperationException($"Client with ID {clientId} not found");
        }

        // Fetch allocations
        var allocations = await _context.ClientAssetAllocations
            .AsNoTracking()
            .Where(a => a.ClientId == clientId && a.EndDate == null)
            .ToListAsync(ct);

        // Fetch wallet balances for crypto assets
        var walletAllocations = allocations.Where(a => a.AssetType == "Wallet").ToList();
        var walletBalances = new List<(string WalletAddress, string TokenSymbol, decimal Balance, decimal UsdValue)>();

        foreach (var allocation in walletAllocations)
        {
            var wallet = await _context.CustodyWallets
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.Id == allocation.AssetId, ct);

            if (wallet != null)
            {
                var balances = await _context.WalletBalances
                    .AsNoTracking()
                    .Where(b => b.WalletId == allocation.AssetId)
                    .ToListAsync(ct);

                foreach (var balance in balances)
                {
                    var allocatedBalance = allocation.AllocationType == "Percentage"
                        ? balance.Balance * (allocation.AllocationValue / 100)
                        : allocation.AllocationValue;

                    var usdPrice = balance.BalanceUsd.HasValue && balance.Balance > 0
                        ? balance.BalanceUsd.Value / balance.Balance
                        : 0;

                    walletBalances.Add((
                        wallet.WalletAddress,
                        balance.TokenSymbol,
                        allocatedBalance,
                        allocatedBalance * usdPrice
                    ));
                }
            }
        }

        // Fetch account balances for traditional assets
        var accountAllocations = allocations.Where(a => a.AssetType == "Account").ToList();
        var accountBalances = new List<(string AccountName, string Currency, decimal Balance)>();

        foreach (var allocation in accountAllocations)
        {
            var account = await _context.TraditionalAccounts
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == allocation.AssetId, ct);

            if (account != null)
            {
                var balances = await _context.AccountBalances
                    .AsNoTracking()
                    .Where(b => b.AccountId == allocation.AssetId)
                    .ToListAsync(ct);

                foreach (var balance in balances)
                {
                    var allocatedBalance = allocation.AllocationType == "Percentage"
                        ? balance.Amount * (allocation.AllocationValue / 100)
                        : allocation.AllocationValue;

                    accountBalances.Add((
                        account.Label ?? account.InstitutionName ?? "Unknown",
                        balance.Currency,
                        allocatedBalance
                    ));
                }
            }
        }

        var totalCryptoValue = walletBalances.Sum(b => b.UsdValue);
        var totalTraditionalValue = accountBalances.Sum(b => b.Balance); // Assuming USD for simplification

        List<(DateTime Date, string Type, string Asset, decimal Amount, decimal Value)>? transactions = null;

        if (includeTransactions)
        {
            var assetIds = allocations.Select(a => a.AssetId).ToList();
            var txs = await _context.Transactions
                .AsNoTracking()
                .Where(t => assetIds.Contains(t.AssetId))
                .OrderByDescending(t => t.TransactionDate)
                .Take(20)
                .ToListAsync(ct);

            transactions = txs.Select(t => (
                t.TransactionDate,
                t.Direction, // IN, OUT, INTERNAL
                t.TokenSymbol ?? t.Category ?? "Unknown",
                t.Amount,
                t.AmountUsd ?? 0
            )).ToList();
        }

        // Generate PDF
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(11).FontFamily(Fonts.Calibri));

                page.Header().Row(row =>
                {
                    row.RelativeItem().Column(column =>
                    {
                        column.Item().Text("Portfolio Report").FontSize(24).Bold().FontColor(Colors.Blue.Darken2);
                        column.Item().Text($"Client: {client.Name}").FontSize(14);
                        column.Item().Text($"Email: {client.Email}").FontSize(10).FontColor(Colors.Grey.Darken1);
                    });

                    row.ConstantItem(100).AlignRight().Column(column =>
                    {
                        column.Item().Text($"Generated").FontSize(9).FontColor(Colors.Grey.Darken1);
                        column.Item().Text($"{DateTime.UtcNow:yyyy-MM-dd}").FontSize(10);
                        column.Item().Text($"{DateTime.UtcNow:HH:mm} UTC").FontSize(9).FontColor(Colors.Grey.Darken1);
                    });
                });

                page.Content().PaddingVertical(1, Unit.Centimetre).Column(column =>
                {
                    column.Spacing(15);

                    // Summary Section
                    column.Item().Row(row =>
                    {
                        row.RelativeItem().Background(Colors.Blue.Lighten4).Padding(10).Column(col =>
                        {
                            col.Item().Text("Total Crypto Assets").FontSize(10).FontColor(Colors.Grey.Darken2);
                            col.Item().Text($"${totalCryptoValue:N2}").FontSize(18).Bold().FontColor(Colors.Blue.Darken2);
                        });

                        row.Spacing(10);

                        row.RelativeItem().Background(Colors.Green.Lighten4).Padding(10).Column(col =>
                        {
                            col.Item().Text("Total Traditional Assets").FontSize(10).FontColor(Colors.Grey.Darken2);
                            col.Item().Text($"${totalTraditionalValue:N2}").FontSize(18).Bold().FontColor(Colors.Green.Darken2);
                        });

                        row.Spacing(10);

                        row.RelativeItem().Background(Colors.Orange.Lighten4).Padding(10).Column(col =>
                        {
                            col.Item().Text("Total Portfolio Value").FontSize(10).FontColor(Colors.Grey.Darken2);
                            col.Item().Text($"${totalCryptoValue + totalTraditionalValue:N2}").FontSize(18).Bold().FontColor(Colors.Orange.Darken2);
                        });
                    });

                    // Crypto Holdings Section
                    if (walletBalances.Any())
                    {
                        column.Item().Text("Crypto Holdings").FontSize(16).Bold();
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(2);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Wallet").Bold();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Token").Bold();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).AlignRight().Text("Balance").Bold();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).AlignRight().Text("USD Value").Bold();
                            });

                            foreach (var balance in walletBalances)
                            {
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten1).Padding(5)
                                    .Text($"{balance.WalletAddress[..8]}...{balance.WalletAddress[^6..]}").FontSize(9);
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten1).Padding(5)
                                    .Text(balance.TokenSymbol);
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten1).Padding(5)
                                    .AlignRight().Text($"{balance.Balance:N6}");
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten1).Padding(5)
                                    .AlignRight().Text($"${balance.UsdValue:N2}");
                            }
                        });
                    }

                    // Traditional Holdings Section
                    if (accountBalances.Any())
                    {
                        column.Item().Text("Traditional Holdings").FontSize(16).Bold();
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(4);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(3);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Account").Bold();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Currency").Bold();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).AlignRight().Text("Balance").Bold();
                            });

                            foreach (var balance in accountBalances)
                            {
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten1).Padding(5)
                                    .Text(balance.AccountName);
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten1).Padding(5)
                                    .Text(balance.Currency);
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten1).Padding(5)
                                    .AlignRight().Text($"{balance.Balance:N2}");
                            }
                        });
                    }

                    // Recent Transactions Section
                    if (includeTransactions && transactions != null && transactions.Any())
                    {
                        column.Item().Text("Recent Transactions (Last 20)").FontSize(16).Bold();
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(2);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Date").Bold();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Type").Bold();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Asset").Bold();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).AlignRight().Text("Amount").Bold();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).AlignRight().Text("Value").Bold();
                            });

                            foreach (var tx in transactions)
                            {
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten1).Padding(5)
                                    .Text($"{tx.Date:yyyy-MM-dd}").FontSize(9);
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten1).Padding(5)
                                    .Text(tx.Type).FontSize(9);
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten1).Padding(5)
                                    .Text(tx.Asset).FontSize(9);
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten1).Padding(5)
                                    .AlignRight().Text($"{tx.Amount:N4}").FontSize(9);
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten1).Padding(5)
                                    .AlignRight().Text($"${tx.Value:N2}").FontSize(9);
                            }
                        });
                    }

                    // Disclaimer
                    column.Item().PaddingTop(20).Text(text =>
                    {
                        text.Span("Note: ").Bold().FontSize(9);
                        text.Span("This report is for informational purposes only. Values shown are allocated portions based on client allocation percentages. " +
                                 "Actual balances in custody wallets/accounts may differ. Please verify all information independently.")
                            .FontSize(8).FontColor(Colors.Grey.Darken1);
                    });
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Page ");
                    x.CurrentPageNumber();
                    x.Span(" of ");
                    x.TotalPages();
                });
            });
        });

        return document.GeneratePdf();
    }

    public async Task<byte[]> GeneratePerformanceReportAsync(Guid clientId, DateTime fromDate, DateTime toDate, CancellationToken ct = default)
    {
        _logger.LogInformation("Generating performance report for client {ClientId} from {FromDate} to {ToDate}",
            clientId, fromDate, toDate);

        var client = await _context.Clients
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == clientId, ct);

        if (client == null)
        {
            throw new InvalidOperationException($"Client with ID {clientId} not found");
        }

        // Fetch performance metrics
        var metrics = await _context.PerformanceMetrics
            .AsNoTracking()
            .Where(m => m.ClientId == clientId && m.CalculatedAt >= fromDate && m.CalculatedAt <= toDate)
            .OrderBy(m => m.CalculatedAt)
            .ToListAsync(ct);

        var latestMetric = metrics.LastOrDefault();
        var firstMetric = metrics.FirstOrDefault();

        var totalReturn = latestMetric != null && firstMetric != null && firstMetric.TotalValueUsd > 0
            ? ((latestMetric.TotalValueUsd - firstMetric.TotalValueUsd) / firstMetric.TotalValueUsd * 100)
            : 0;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(11).FontFamily(Fonts.Calibri));

                page.Header().Row(row =>
                {
                    row.RelativeItem().Column(column =>
                    {
                        column.Item().Text("Performance Report").FontSize(24).Bold().FontColor(Colors.Blue.Darken2);
                        column.Item().Text($"Client: {client.Name}").FontSize(14);
                        column.Item().Text($"Period: {fromDate:yyyy-MM-dd} to {toDate:yyyy-MM-dd}").FontSize(10).FontColor(Colors.Grey.Darken1);
                    });

                    row.ConstantItem(100).AlignRight().Column(column =>
                    {
                        column.Item().Text($"Generated").FontSize(9).FontColor(Colors.Grey.Darken1);
                        column.Item().Text($"{DateTime.UtcNow:yyyy-MM-dd}").FontSize(10);
                        column.Item().Text($"{DateTime.UtcNow:HH:mm} UTC").FontSize(9).FontColor(Colors.Grey.Darken1);
                    });
                });

                page.Content().PaddingVertical(1, Unit.Centimetre).Column(column =>
                {
                    column.Spacing(15);

                    // Summary Metrics
                    column.Item().Text("Performance Summary").FontSize(16).Bold();
                    column.Item().Row(row =>
                    {
                        row.RelativeItem().Background(Colors.Blue.Lighten4).Padding(10).Column(col =>
                        {
                            col.Item().Text("Total Return").FontSize(10).FontColor(Colors.Grey.Darken2);
                            col.Item().Text($"{totalReturn:N2}%").FontSize(18).Bold()
                                .FontColor(totalReturn >= 0 ? Colors.Green.Darken2 : Colors.Red.Darken2);
                        });

                        row.Spacing(10);

                        row.RelativeItem().Background(Colors.Green.Lighten4).Padding(10).Column(col =>
                        {
                            col.Item().Text("Current Value").FontSize(10).FontColor(Colors.Grey.Darken2);
                            col.Item().Text($"${latestMetric?.TotalValueUsd ?? 0:N2}").FontSize(18).Bold().FontColor(Colors.Green.Darken2);
                        });

                        row.Spacing(10);

                        row.RelativeItem().Background(Colors.Orange.Lighten4).Padding(10).Column(col =>
                        {
                            col.Item().Text("P&L").FontSize(10).FontColor(Colors.Grey.Darken2);
                            var pnl = latestMetric?.ProfitLoss ?? 0;
                            col.Item().Text($"${pnl:N2}").FontSize(18).Bold()
                                .FontColor(pnl >= 0 ? Colors.Green.Darken2 : Colors.Red.Darken2);
                        });
                    });

                    // Performance History
                    if (metrics.Any())
                    {
                        column.Item().Text("Performance History").FontSize(16).Bold();
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(2);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Date").Bold();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).AlignRight().Text("Total Value").Bold();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).AlignRight().Text("ROI %").Bold();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).AlignRight().Text("P&L").Bold();
                            });

                            foreach (var metric in metrics.TakeLast(15))
                            {
                                var roi = metric.Roi ?? 0;
                                var pnl = metric.ProfitLoss ?? 0;

                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten1).Padding(5)
                                    .Text($"{metric.CalculatedAt:yyyy-MM-dd}").FontSize(9);
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten1).Padding(5)
                                    .AlignRight().Text($"${metric.TotalValueUsd:N2}").FontSize(9);
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten1).Padding(5)
                                    .AlignRight().Text($"{roi:N2}%").FontSize(9)
                                    .FontColor(roi >= 0 ? Colors.Green.Darken2 : Colors.Red.Darken2);
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten1).Padding(5)
                                    .AlignRight().Text($"${pnl:N2}").FontSize(9)
                                    .FontColor(pnl >= 0 ? Colors.Green.Darken2 : Colors.Red.Darken2);
                            }
                        });
                    }
                    else
                    {
                        column.Item().Text("No performance data available for the selected period.")
                            .FontColor(Colors.Grey.Darken1).Italic();
                    }

                    // Disclaimer
                    column.Item().PaddingTop(20).Text(text =>
                    {
                        text.Span("Note: ").Bold().FontSize(9);
                        text.Span("Past performance is not indicative of future results. All figures are estimates based on available data. " +
                                 "Please consult with a financial advisor before making investment decisions.")
                            .FontSize(8).FontColor(Colors.Grey.Darken1);
                    });
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Page ");
                    x.CurrentPageNumber();
                    x.Span(" of ");
                    x.TotalPages();
                });
            });
        });

        return document.GeneratePdf();
    }
}
