using System.Collections.Concurrent;

namespace Wavelength.Playlists.Domain;

internal sealed record Playlist(string Id, string Name, string Description, IReadOnlyList<string> TrackIds);

/// <summary>Minimal in-memory playlist store for slice 1 (read only). Editing comes in a later slice.</summary>
internal sealed class PlaylistStore
{
    private readonly ConcurrentDictionary<string, Playlist> _playlists = new(StringComparer.Ordinal);

    public void Save(Playlist playlist) => _playlists[playlist.Id] = playlist;

    public Playlist? Find(string id) => _playlists.GetValueOrDefault(id);

    public IReadOnlyList<Playlist> All() => _playlists.Values.OrderBy(p => p.Name, StringComparer.Ordinal).ToList();

    public static void SeedDemo(PlaylistStore store)
    {
        store.Save(new Playlist("pl-commute", "Commute: Taxi Rank to Town", "Invented demo playlist for the morning ride.", ["trk-night-bus", "trk-tidewater-2", "trk-cafe-kwasa", "trk-signal-fires", "trk-kelp-waltz"]));
        store.Save(new Playlist("pl-late", "Late Kitchen", "Invented demo playlist for slow evenings.", ["trk-salt-road", "trk-mbira-dusk", "trk-harbour-light", "trk-protea-rain"]));
    }
}
