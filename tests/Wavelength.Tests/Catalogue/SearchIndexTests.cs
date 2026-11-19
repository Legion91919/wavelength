using Wavelength.Catalogue.Domain;
using Wavelength.Catalogue.Search;
using Xunit;

namespace Wavelength.Catalogue.Tests;

public class SearchIndexTests
{
    private static readonly SearchIndex Index = SearchIndex.Build(DemoCatalogue.Create());

    [Fact]
    public void Search_returns_matching_tracks_artists_and_albums()
    {
        var hits = Index.Search("meridien", 10);

        Assert.Contains(hits.Artists, a => a.Name == "Dune Méridien");
        Assert.Contains(hits.Albums, a => a.Title == "Méridien Nights");
        Assert.Contains(hits.Tracks, t => t.Title == "Salt Road Lullaby");
    }

    [Theory]
    [InlineData("MÉRIDIEN")]
    [InlineData("meridien")]
    [InlineData("Méri")]
    public void Search_is_accent_and_case_insensitive_with_prefix_matching(string query)
    {
        var hits = Index.Search(query, 10);

        Assert.Contains(hits.Artists, a => a.Id == "art-dune");
    }

    [Fact]
    public void Query_with_accent_matches_text_without_one()
    {
        var hits = Index.Search("café", 10);

        Assert.Contains(hits.Tracks, t => t.Id == "trk-cafe-kwasa");
        Assert.Contains(Index.Search("cafe", 10).Tracks, t => t.Id == "trk-cafe-kwasa");
    }

    [Fact]
    public void Every_query_word_must_match()
    {
        var hits = Index.Search("night bus", 10);

        var track = Assert.Single(hits.Tracks);
        Assert.Equal("trk-night-bus", track.Id);
    }

    [Fact]
    public void Exact_title_ranks_above_partial_matches()
    {
        var hits = Index.Search("lantern season", 10);

        Assert.Equal("trk-lantern-season", hits.Tracks[0].Id);
        Assert.Equal("alb-lantern", hits.Albums[0].Id);
    }

    [Fact]
    public void Unknown_term_returns_nothing()
    {
        var hits = Index.Search("xqzzt", 10);

        Assert.Empty(hits.Tracks);
        Assert.Empty(hits.Artists);
        Assert.Empty(hits.Albums);
    }

    [Fact]
    public void Limit_caps_each_group()
    {
        var hits = Index.Search("t", 2);

        Assert.True(hits.Tracks.Count <= 2);
        Assert.True(hits.Artists.Count <= 2);
        Assert.True(hits.Albums.Count <= 2);
    }
}
