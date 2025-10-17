using Carter;
using MediatR;

namespace ApiService.Features.Export.GetExportJob;

public class GetExportJobEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/export/jobs/{jobId:guid}", async (
            Guid jobId,
            ISender sender,
            CancellationToken ct = default) =>
        {
            var query = new GetExportJobQuery(jobId);
            var result = await sender.Send(query, ct);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.NotFound(new { error = result.Error });
        })
        .WithName("GetExportJob")
        .WithTags("Export")
        .WithOpenApi()
        .Produces<ExportJobDto>(StatusCodes.Status200OK)
        .Produces<object>(StatusCodes.Status404NotFound);
    }
}
