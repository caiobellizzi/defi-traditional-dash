using Carter;
using MediatR;

namespace ApiService.Features.Export.ExportPerformanceExcel;

public class ExportPerformanceExcelEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/export/performance/excel", async (
            ExportPerformanceExcelRequest request,
            ISender sender,
            CancellationToken ct = default) =>
        {
            var command = new ExportPerformanceExcelCommand(
                request.ClientId,
                request.FromDate,
                request.ToDate);
            var result = await sender.Send(command, ct);

            return result.IsSuccess
                ? Results.Accepted($"/api/export/jobs/{result.Value!.JobId}", result.Value)
                : Results.BadRequest(new { error = result.Error });
        })
        .WithName("ExportPerformanceExcel")
        .WithTags("Export")
        .WithOpenApi()
        .Produces<ExportJobDto>(StatusCodes.Status202Accepted)
        .Produces<object>(StatusCodes.Status400BadRequest);
    }
}

public record ExportPerformanceExcelRequest(
    Guid? ClientId,
    DateTime FromDate,
    DateTime ToDate);
