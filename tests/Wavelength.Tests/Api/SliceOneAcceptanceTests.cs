using System.Net;
using System.Text.Json;
using Wavelength.Tests.Support;
using Xunit;

namespace Wavelength.Api.Tests;

/// <summary>One test per slice-1 acceptance criterion, against the real HTTP API with seeded demo data.</summary>
public class SliceOneAcceptanceTests
{
    private const string Track = "trk-night-bus";
    private const string Label = "lbl-saltpan";
    private const string Statement = "/api/labels/lbl-saltpan/statement?from=2026-11-01&to=2026-11-30";

    [Fact]
    public async Task Given_a_search_term_then_matching_tracks_artists_and_albums_are_returned()
    {
        await using var host = await ApiHost.StartAsync();

        var body = await host.GetJsonAsync("/api/search?q=meridien");

        Assert.Contains(body.GetProperty("artists").EnumerateArray(), a => a.GetProperty("name").GetString() == "Dune Méridien");
        Assert.Contains(body.GetProperty("albums").EnumerateArray(), a => a.GetProperty("title").GetString() == "Méridien Nights");
        Assert.Contains(body.GetProperty("tracks").EnumerateArray(), t => t.GetProperty("id").GetString() == "trk-salt-road");
        var track = body.GetProperty("tracks")[0];
        Assert.StartsWith("/cdn/audio/", track.GetProperty("audioUrl").GetString());
        Assert.Equal(Label, track.GetProperty("labelId").GetString());
    }

    [Fact]
    public async Task Given_a_search_term_without_accents_then_accented_names_match()
    {
        await using var host = await ApiHost.StartAsync();

        var body = await host.GetJsonAsync("/api/search?q=thembe");

        Assert.Contains(body.GetProperty("artists").EnumerateArray(), a => a.GetProperty("name").GetString() == "Thembé & The Tide");
    }

    [Fact]
    public async Task Given_a_play_that_lasts_31_seconds_then_exactly_one_royalty_play_is_recorded_for_the_label()
    {
        await using var host = await ApiHost.StartAsync();

        var response = await host.PostPlayAsync(Guid.NewGuid(), Track, 31);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var statement = await host.GetJsonAsync(Statement);
        Assert.Equal(1, statement.GetProperty("totalPlays").GetInt32());
        Assert.Equal(0.05m, statement.GetProperty("totalAmount").GetDecimal());
        var ledger = await host.GetJsonAsync("/api/labels/lbl-saltpan/ledger?from=2026-11-01&to=2026-11-30");
        Assert.Equal(1, ledger.GetArrayLength());
    }

    [Fact]
    public async Task Given_the_same_play_event_sent_twice_then_it_is_still_counted_once()
    {
        await using var host = await ApiHost.StartAsync();
        var playId = Guid.NewGuid();

        var first = await host.PostPlayAsync(playId, Track, 31);
        var second = await host.PostPlayAsync(playId, Track, 31);
        var end = await host.PostPlayAsync(playId, Track, 221, "ended");

        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Equal(HttpStatusCode.OK, second.StatusCode);
        Assert.Equal(HttpStatusCode.OK, end.StatusCode);
        var statement = await host.GetJsonAsync(Statement);
        Assert.Equal(1, statement.GetProperty("totalPlays").GetInt32());
    }

    [Fact]
    public async Task Given_a_play_of_10_seconds_then_no_royalty_play_is_recorded()
    {
        await using var host = await ApiHost.StartAsync();

        var response = await host.PostPlayAsync(Guid.NewGuid(), Track, 10, "skipped");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
        Assert.False(body.GetProperty("counted").GetBoolean());
        var statement = await host.GetJsonAsync(Statement);
        Assert.Equal(0, statement.GetProperty("totalPlays").GetInt32());
    }

    [Fact]
    public async Task Statement_for_a_period_reconciles_exactly_with_the_ledger()
    {
        await using var host = await ApiHost.StartAsync();
        string[] tracks = ["trk-night-bus", "trk-salt-road", "trk-lanterns-saltpan", "trk-long-road"];
        for (var i = 0; i < 23; i++)
        {
            host.Clock.Advance(TimeSpan.FromHours(7));
            await host.PostPlayAsync(Guid.NewGuid(), tracks[i % tracks.Length], 30 + i);
        }

        var statement = await host.GetJsonAsync(Statement);
        var ledger = await host.GetJsonAsync("/api/labels/lbl-saltpan/ledger?from=2026-11-01&to=2026-11-30");

        var ledgerTotal = ledger.EnumerateArray().Sum(e => e.GetProperty("amount").GetDecimal());
        Assert.Equal(23, ledger.GetArrayLength());
        Assert.Equal(23, statement.GetProperty("totalPlays").GetInt32());
        Assert.Equal(ledgerTotal, statement.GetProperty("totalAmount").GetDecimal());
        Assert.Equal(1.15m, ledgerTotal);
        Assert.True(statement.GetProperty("reconciliation").GetProperty("reconciled").GetBoolean());
    }
}
