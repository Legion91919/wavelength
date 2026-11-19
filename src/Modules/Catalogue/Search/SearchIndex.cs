using Wavelength.Catalogue.Domain;

namespace Wavelength.Catalogue.Search;

internal sealed record SearchHits(IReadOnlyList<Track> Tracks, IReadOnlyList<Artist> Artists, IReadOnlyList<Album> Albums);

/// <summary>
/// In-memory read model for search (the advisor's CachedReadModel). Built once per catalogue snapshot;
/// every query word must prefix-match a word of the document. Primary-field matches outrank secondary ones.
/// </summary>
internal sealed class SearchIndex
{
    private readonly Entry<Track>[] _tracks;
    private readonly Entry<Artist>[] _artists;
    private readonly Entry<Album>[] _albums;

    private SearchIndex(Entry<Track>[] tracks, Entry<Artist>[] artists, Entry<Album>[] albums)
    {
        _tracks = tracks;
        _artists = artists;
        _albums = albums;
    }

    public int DocumentCount => _tracks.Length + _artists.Length + _albums.Length;

    public static SearchIndex Build(CatalogueSnapshot catalogue)
    {
        var tracks = catalogue.Tracks.Values.Select(t =>
        {
            var artist = catalogue.Artists[t.ArtistId];
            var album = catalogue.Albums[t.AlbumId];
            return Entry<Track>.Create(t, t.Title, $"{artist.Name} {album.Title}");
        }).ToArray();
        var artists = catalogue.Artists.Values
            .Select(a => Entry<Artist>.Create(a, a.Name, catalogue.LabelOf(a).Name))
            .ToArray();
        var albums = catalogue.Albums.Values
            .Select(a => Entry<Album>.Create(a, a.Title, catalogue.Artists[a.ArtistId].Name))
            .ToArray();
        return new SearchIndex(tracks, artists, albums);
    }

    public SearchHits Search(string query, int limit)
    {
        var normalised = TextNormaliser.Normalise(query);
        var words = normalised.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (words.Length == 0)
        {
            return new SearchHits([], [], []);
        }

        return new SearchHits(Rank(_tracks, words, normalised, limit), Rank(_artists, words, normalised, limit), Rank(_albums, words, normalised, limit));
    }

    private static List<T> Rank<T>(Entry<T>[] entries, string[] words, string normalised, int limit) =>
        entries
            .Select(e => (Entry: e, Score: e.Score(words, normalised)))
            .Where(x => x.Score > 0)
            .OrderByDescending(x => x.Score)
            .ThenBy(x => x.Entry.PrimaryText.Length)
            .ThenBy(x => x.Entry.PrimaryText, StringComparer.Ordinal)
            .Take(limit)
            .Select(x => x.Entry.Item)
            .ToList();

    private sealed record Entry<T>(T Item, string PrimaryText, string[] Primary, string[] Secondary)
    {
        public static Entry<T> Create(T item, string primary, string secondary)
        {
            var primaryText = TextNormaliser.Normalise(primary);
            return new Entry<T>(item, primaryText, primaryText.Split(' ', StringSplitOptions.RemoveEmptyEntries), TextNormaliser.Tokens(secondary));
        }

        /// <summary>0 means "no match": some query word matched nothing.</summary>
        public int Score(string[] words, string normalisedQuery)
        {
            var score = 0;
            foreach (var word in words)
            {
                if (Array.IndexOf(Primary, word) >= 0)
                {
                    score += 4;
                }
                else if (Primary.Any(p => p.StartsWith(word, StringComparison.Ordinal)))
                {
                    score += 3;
                }
                else if (Array.IndexOf(Secondary, word) >= 0)
                {
                    score += 2;
                }
                else if (Secondary.Any(s => s.StartsWith(word, StringComparison.Ordinal)))
                {
                    score += 1;
                }
                else
                {
                    return 0;
                }
            }

            if (PrimaryText == normalisedQuery)
            {
                score += 10;
            }
            else if (PrimaryText.StartsWith(normalisedQuery, StringComparison.Ordinal))
            {
                score += 5;
            }

            return score;
        }
    }
}
