using Carter;
using MediatR;

namespace ApiService.Features.System.UpdateConfiguration;

public class UpdateConfigurationEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/api/system/config", async (
            UpdateConfigurationRequest request,
            ISender sender,
            CancellationToken ct = default) =>
        {
            var command = new UpdateConfigurationCommand(request.Settings);
            var result = await sender.Send(command, ct);

            return result.IsSuccess
                ? Results.Ok(new { updated = result.Value })
                : Results.BadRequest(new { error = result.Error });
        })
        .WithName("UpdateSystemConfiguration")
        .WithTags("System")
        .WithOpenApi()
        .Produces<object>(StatusCodes.Status200OK)
        .Produces<object>(StatusCodes.Status400BadRequest);
    }
}

public record UpdateConfigurationRequest(Dictionary<string, string> Settings);
