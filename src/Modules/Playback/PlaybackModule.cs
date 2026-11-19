using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Wavelength.Playback.Application;
using Wavelength.Playback.Contracts;

namespace Wavelength.Playback;

/// <summary>Playback module: play sessions and the idempotent play-event intake.</summary>
public static class PlaybackModule
{
    public static IServiceCollection AddPlaybackModule(this IServiceCollection services)
    {
        services.AddSingleton<IPlaySessionStore, InMemoryPlaySessionStore>();
        services.AddSingleton<PlayEventService>();
        return services;
    }

    public static IEndpointRouteBuilder MapPlaybackEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/plays", (PlayEventRequest request, PlayEventService service) =>
        {
            var outcome = service.Record(request);
            return outcome.Status switch
            {
                PlayEventStatus.Accepted => Results.Ok(outcome.Response),
                PlayEventStatus.Invalid => Results.ValidationProblem(outcome.Errors!),
                PlayEventStatus.TrackNotFound => Results.Problem(statusCode: StatusCodes.Status404NotFound, title: "Track not found", detail: outcome.Detail),
                _ => Results.Problem(statusCode: StatusCodes.Status409Conflict, title: "Play ID conflict", detail: outcome.Detail),
            };
        }).WithTags("Playback");
        return app;
    }
}
