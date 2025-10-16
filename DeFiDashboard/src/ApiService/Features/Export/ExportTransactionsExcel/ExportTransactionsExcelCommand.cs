using ApiService.Features.Alerts.GetList;
using ApiService.Features.Export.ExportPortfolioPdf;
using MediatR;

namespace ApiService.Features.Export.ExportTransactionsExcel;

public record ExportTransactionsExcelCommand(
    Guid? ClientId = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    string? TransactionType = null
) : IRequest<Result<ExportJobDto>>;
