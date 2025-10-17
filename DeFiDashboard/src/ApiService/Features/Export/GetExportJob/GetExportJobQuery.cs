using ApiService.Features.Alerts.GetList;
using ApiService.Features.Export.ExportPortfolioPdf;
using MediatR;

namespace ApiService.Features.Export.GetExportJob;

public record GetExportJobQuery(Guid JobId) : IRequest<Result<ExportJobDto>>;
