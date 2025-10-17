using Carter;
using MediatR;

namespace ApiService.Features.Export.DownloadExport;

public class DownloadExportEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/export/jobs/{jobId:guid}/download", async (
            Guid jobId,
            ISender sender,
            CancellationToken ct = default) =>
        {
            var query = new DownloadExportQuery(jobId);
            var result = await sender.Send(query, ct);

            if (!result.IsSuccess)
            {
                return Results.NotFound(new { error = result.Error });
            }

            return Results.File(
                result.Value!.FileContent,
                result.Value.ContentType,
                result.Value.FileName);
        })
        .WithName("DownloadExport")
        .WithTags("Export")
        .WithOpenApi()
        .Produces(StatusCodes.Status200OK)
        .Produces<object>(StatusCodes.Status404NotFound);
    }
}
