using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Wavelength.Playlists.Contracts;
using Wavelength.Playlists.Domain;
using Wavelength.SharedKernel;

namespace Wavelength.Playlists;

/// <summary>Playlists module (minimal in slice 1): read-only curated lists that reference catalogue tracks by ID.</summary>
public static class PlaylistsModule
{
    public static IServiceCollection AddPlaylistsModule(this IServiceCollection services, bool seedDemoData)
    {
        services.AddSingleton(_ =>
        {
            var store = new PlaylistStore();
            if (seedDemoData)
            {
                PlaylistStore.SeedDemo(store);
            }

            return store;
        });
        return services;
    }

    public static IEndpointRouteBuilder MapPlaylistsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/playlists").WithTags("Playlists");
        group.MapGet("/", (PlaylistStore store) =>
            store.All().Select(p => new PlaylistSummary(p.Id, p.Name, p.Description, p.TrackIds.Count)).ToList());
        group.MapGet("/{id}", (string id, PlaylistStore store, ICatalogueDirectory directory) =>
        {
            var playlist = store.Find(id);
            if (playlist is null)
            {
                return Results.Problem(statusCode: StatusCodes.Status404NotFound, title: "Playlist not found");
            }

            var tracks = playlist.TrackIds
                .Select(directory.FindTrack)
                .OfType<TrackRef>()
                .Select(t => new PlaylistTrack(t.TrackId, t.Title, t.DurationSeconds))
                .ToList();
            return Results.Ok(new PlaylistDetail(playlist.Id, playlist.Name, playlist.Description, tracks));
        });
        return app;
    }
}
