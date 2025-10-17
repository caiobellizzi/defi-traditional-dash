using Carter;
using MediatR;

namespace ApiService.Features.Export.ExportPortfolioPdf;

public class ExportPortfolioPdfEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/export/portfolio/pdf", async (
            ExportPortfolioPdfRequest request,
            ISender sender,
            CancellationToken ct = default) =>
        {
            var command = new ExportPortfolioPdfCommand(request.ClientId, request.IncludeTransactions);
            var result = await sender.Send(command, ct);

            return result.IsSuccess
                ? Results.Accepted($"/api/export/jobs/{result.Value!.JobId}", result.Value)
                : Results.BadRequest(new { error = result.Error });
        })
        .WithName("ExportPortfolioPdf")
        .WithTags("Export")
        .WithOpenApi()
        .Produces<ExportJobDto>(StatusCodes.Status202Accepted)
        .Produces<object>(StatusCodes.Status400BadRequest);
    }
}

public record ExportPortfolioPdfRequest(Guid ClientId, bool IncludeTransactions = false);
