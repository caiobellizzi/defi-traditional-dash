using ApiService.Features.Alerts.GetList;
using ApiService.Features.Export.ExportPortfolioPdf;
using MediatR;

namespace ApiService.Features.Export.ExportPerformanceExcel;

public record ExportPerformanceExcelCommand(
    Guid? ClientId,
    DateTime FromDate,
    DateTime ToDate
) : IRequest<Result<ExportJobDto>>;
