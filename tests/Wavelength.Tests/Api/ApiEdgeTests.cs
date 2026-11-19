using System.Net;
using System.Net.Http.Json;
using Wavelength.Tests.Support;
using Xunit;

namespace Wavelength.Api.Tests;

public class ApiEdgeTests
{
    [Fact]
    public async Task Invalid_play_event_returns_validation_problem_details()
    {
        await using var host = await ApiHost.StartAsync();

        var response = await host.Client.PostAsJsonAsync("/api/plays", new { playId = "nope", trackId = "trk-night-bus", listenedSeconds = 31, reason = "threshold" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        Assert.Contains("playId", await response.Content.ReadAsStringAsync(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task Play_for_unknown_track_returns_404_problem()
    {
        await using var host = await ApiHost.StartAsync();

        var response = await host.PostPlayAsync(Guid.NewGuid(), "trk-missing", 31);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task Reusing_a_play_id_for_another_track_returns_409()
    {
        await using var host = await ApiHost.StartAsync();
        var playId = Guid.NewGuid();
        await host.PostPlayAsync(playId, "trk-night-bus", 31);

        var response = await host.PostPlayAsync(playId, "trk-salt-road", 31);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Theory]
    [InlineData("/api/search")]
    [InlineData("/api/search?q=%20")]
    [InlineData("/api/search?q=abc&limit=0")]
    [InlineData("/api/labels/lbl-saltpan/statement?from=2026-11-30&to=2026-11-01")]
    [InlineData("/api/labels/lbl-saltpan/statement?from=2026-11-01")]
    public async Task Bad_queries_return_400(string url)
    {
        await using var host = await ApiHost.StartAsync();

        var response = await host.Client.GetAsync(url);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Statement_for_unknown_label_returns_404()
    {
        await using var host = await ApiHost.StartAsync();

        var response = await host.Client.GetAsync("/api/labels/lbl-nope/statement?from=2026-11-01&to=2026-11-30");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Health_endpoints_report_healthy()
    {
        await using var host = await ApiHost.StartAsync();

        Assert.Equal(HttpStatusCode.OK, (await host.Client.GetAsync("/health/live")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await host.Client.GetAsync("/health/ready")).StatusCode);
    }

    [Fact]
    public async Task Cdn_serves_track_audio_with_long_lived_cache_headers()
    {
        await using var host = await ApiHost.StartAsync();
        var search = await host.GetJsonAsync("/api/search?q=night%20bus");
        var audioUrl = search.GetProperty("tracks")[0].GetProperty("audioUrl").GetString()!;

        var response = await host.Client.GetAsync(audioUrl);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("audio/mpeg", response.Content.Headers.ContentType?.MediaType);
        Assert.Contains("immutable", response.Headers.CacheControl?.ToString() ?? string.Empty, StringComparison.Ordinal);
        Assert.True((await response.Content.ReadAsByteArrayAsync()).Length > 1000);
    }

    [Fact]
    public async Task Album_and_artist_lookups_return_their_tracks()
    {
        await using var host = await ApiHost.StartAsync();

        var album = await host.GetJsonAsync("/api/albums/alb-meridien");
        var artist = await host.GetJsonAsync("/api/artists/art-dune");

        Assert.Equal(3, album.GetProperty("tracks").GetArrayLength());
        Assert.Equal(3, artist.GetProperty("tracks").GetArrayLength());
        Assert.Equal(HttpStatusCode.NotFound, (await host.Client.GetAsync("/api/albums/alb-nope")).StatusCode);
    }

    [Fact]
    public async Task Seeded_playlist_resolves_its_tracks()
    {
        await using var host = await ApiHost.StartAsync();

        var lists = await host.GetJsonAsync("/api/playlists");
        var id = lists[0].GetProperty("id").GetString();
        var list = await host.GetJsonAsync($"/api/playlists/{id}");

        Assert.True(list.GetProperty("tracks").GetArrayLength() > 0);
    }
}
