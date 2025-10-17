using Carter;
using MediatR;

namespace ApiService.Features.Export.ExportTransactionsExcel;

public class ExportTransactionsExcelEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/export/transactions/excel", async (
            ExportTransactionsExcelRequest request,
            ISender sender,
            CancellationToken ct = default) =>
        {
            var command = new ExportTransactionsExcelCommand(
                request.ClientId,
                request.FromDate,
                request.ToDate,
                request.TransactionType);
            var result = await sender.Send(command, ct);

            return result.IsSuccess
                ? Results.Accepted($"/api/export/jobs/{result.Value!.JobId}", result.Value)
                : Results.BadRequest(new { error = result.Error });
        })
        .WithName("ExportTransactionsExcel")
        .WithTags("Export")
        .WithOpenApi()
        .Produces<ExportJobDto>(StatusCodes.Status202Accepted)
        .Produces<object>(StatusCodes.Status400BadRequest);
    }
}

public record ExportTransactionsExcelRequest(
    Guid? ClientId,
    DateTime? FromDate,
    DateTime? ToDate,
    string? TransactionType);
