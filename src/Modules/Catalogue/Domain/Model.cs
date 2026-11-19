namespace Wavelength.Catalogue.Domain;

internal sealed record Label(string Id, string Name);

internal sealed record Artist(string Id, string Name, string LabelId);

internal sealed record Album(string Id, string Title, string ArtistId, int Year);

internal sealed record Track(string Id, string Title, string ArtistId, string AlbumId, int TrackNumber, int DurationSeconds, string AudioFile);

/// <summary>An immutable view of the whole catalogue. Writes (label feeds, slice 3) build a new snapshot.</summary>
internal sealed class CatalogueSnapshot
{
    public CatalogueSnapshot(IEnumerable<Label> labels, IEnumerable<Artist> artists, IEnumerable<Album> albums, IEnumerable<Track> tracks)
    {
        Labels = labels.ToDictionary(l => l.Id, StringComparer.Ordinal);
        Artists = artists.ToDictionary(a => a.Id, StringComparer.Ordinal);
        Albums = albums.ToDictionary(a => a.Id, StringComparer.Ordinal);
        Tracks = tracks.ToDictionary(t => t.Id, StringComparer.Ordinal);
    }

    public static CatalogueSnapshot Empty { get; } = new([], [], [], []);

    public IReadOnlyDictionary<string, Label> Labels { get; }

    public IReadOnlyDictionary<string, Artist> Artists { get; }

    public IReadOnlyDictionary<string, Album> Albums { get; }

    public IReadOnlyDictionary<string, Track> Tracks { get; }

    public Label LabelOf(Artist artist) => Labels[artist.LabelId];

    public IEnumerable<Track> TracksOfAlbum(string albumId) =>
        Tracks.Values.Where(t => t.AlbumId == albumId).OrderBy(t => t.TrackNumber);

    public IEnumerable<Track> TracksOfArtist(string artistId) =>
        Tracks.Values.Where(t => t.ArtistId == artistId).OrderBy(t => t.AlbumId, StringComparer.Ordinal).ThenBy(t => t.TrackNumber);
}
