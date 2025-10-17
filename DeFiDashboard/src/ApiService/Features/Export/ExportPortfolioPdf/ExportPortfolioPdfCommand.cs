using ApiService.Features.Alerts.GetList;
using MediatR;

namespace ApiService.Features.Export.ExportPortfolioPdf;

public record ExportPortfolioPdfCommand(
    Guid ClientId,
    bool IncludeTransactions = false
) : IRequest<Result<ExportJobDto>>;
