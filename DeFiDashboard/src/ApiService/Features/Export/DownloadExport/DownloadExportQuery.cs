using ApiService.Features.Alerts.GetList;
using MediatR;

namespace ApiService.Features.Export.DownloadExport;

public record DownloadExportQuery(Guid JobId) : IRequest<Result<ExportFileDto>>;

public record ExportFileDto
{
    public byte[] FileContent { get; init; } = Array.Empty<byte>();
    public string FileName { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
}
