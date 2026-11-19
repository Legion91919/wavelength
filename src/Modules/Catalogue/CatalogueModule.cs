using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Wavelength.Catalogue.Contracts;
using Wavelength.Catalogue.Domain;
using Wavelength.Catalogue.Infrastructure;
using Wavelength.SharedKernel;

namespace Wavelength.Catalogue;

/// <summary>Catalogue module: labels, artists, albums, tracks and the search read model.</summary>
public static class CatalogueModule
{
    public const int MaxQueryLength = 100;
    public const int MaxLimit = 50;

    public static IServiceCollection AddCatalogueModule(this IServiceCollection services, bool seedDemoData)
    {
        services.AddSingleton(_ =>
        {
            var store = new CatalogueStore();
            if (seedDemoData)
            {
                store.Load(DemoCatalogue.Create());
            }

            return store;
        });
        services.AddSingleton<ICatalogueDirectory>(sp => sp.GetRequiredService<CatalogueStore>());
        services.AddHealthChecks().AddCheck("catalogue-index", () => HealthCheckResult.Healthy("search index built"), ["ready"]);
        return services;
    }

    public static IEndpointRouteBuilder MapCatalogueEndpoints(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("/api").WithTags("Catalogue");

        api.MapGet("/search", Search);
        api.MapGet("/tracks/{id}", (string id, CatalogueStore store) =>
        {
            var s = store.Snapshot;
            return s.Tracks.TryGetValue(id, out var t) ? Results.Ok(t.ToDto(s)) : Results.Problem(statusCode: 404, title: "Track not found");
        });
        api.MapGet("/artists/{id}", (string id, CatalogueStore store) =>
        {
            var s = store.Snapshot;
            return s.Artists.TryGetValue(id, out var a)
                ? Results.Ok(new ArtistDetailDto(a.ToDto(s), s.TracksOfArtist(id).Select(t => t.ToDto(s)).ToList()))
                : Results.Problem(statusCode: 404, title: "Artist not found");
        });
        api.MapGet("/albums/{id}", (string id, CatalogueStore store) =>
        {
            var s = store.Snapshot;
            return s.Albums.TryGetValue(id, out var a)
                ? Results.Ok(new AlbumDetailDto(a.ToDto(s), s.TracksOfAlbum(id).Select(t => t.ToDto(s)).ToList()))
                : Results.Problem(statusCode: 404, title: "Album not found");
        });
        api.MapGet("/labels", (ICatalogueDirectory directory) =>
            directory.Labels().Select(l => new LabelDto(l.LabelId, l.Name)).ToList());

        return app;
    }

    private static Results<Ok<SearchResponse>, ValidationProblem> Search(string? q, int? limit, CatalogueStore store, TimeProvider clock)
    {
        var errors = new Dictionary<string, string[]>();
        var query = q?.Trim() ?? string.Empty;
        if (query.Length == 0)
        {
            errors["q"] = ["Type something to search for."];
        }
        else if (query.Length > MaxQueryLength)
        {
            errors["q"] = [$"Keep searches under {MaxQueryLength} characters."];
        }

        var take = limit ?? 10;
        if (take is < 1 or > MaxLimit)
        {
            errors["limit"] = [$"limit must be between 1 and {MaxLimit}."];
        }

        if (errors.Count > 0)
        {
            return TypedResults.ValidationProblem(errors);
        }

        var started = clock.GetTimestamp();
        var snapshot = store.Snapshot;
        var hits = store.Index.Search(query, take);
        var response = new SearchResponse(
            query,
            hits.Tracks.Select(t => t.ToDto(snapshot)).ToList(),
            hits.Artists.Select(a => a.ToDto(snapshot)).ToList(),
            hits.Albums.Select(a => a.ToDto(snapshot)).ToList(),
            Math.Round(clock.GetElapsedTime(started).TotalMilliseconds, 3));
        return TypedResults.Ok(response);
    }
}
