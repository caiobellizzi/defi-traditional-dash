using Carter;
using MediatR;

namespace ApiService.Features.System.GetConfiguration;

public class GetConfigurationEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/system/config", async (
            ISender sender,
            CancellationToken ct = default) =>
        {
            var query = new GetConfigurationQuery();
            var result = await sender.Send(query, ct);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(new { error = result.Error });
        })
        .WithName("GetSystemConfiguration")
        .WithTags("System")
        .WithOpenApi()
        .Produces<SystemConfigurationDto>(StatusCodes.Status200OK)
        .Produces<object>(StatusCodes.Status400BadRequest);
    }
}
