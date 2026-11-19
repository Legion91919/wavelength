using Wavelength.Catalogue.Domain;
using Wavelength.Catalogue.Search;
using Wavelength.SharedKernel;

namespace Wavelength.Catalogue.Infrastructure;

/// <summary>
/// Thread-safe in-memory catalogue. A new snapshot and its search index are swapped in together,
/// so readers always see a consistent pair. Postgres replaces the snapshot source in slice 2.
/// </summary>
internal sealed class CatalogueStore : ICatalogueDirectory
{
    private State _state = new(CatalogueSnapshot.Empty, SearchIndex.Build(CatalogueSnapshot.Empty));

    public CatalogueSnapshot Snapshot => Volatile.Read(ref _state).Snapshot;

    public SearchIndex Index => Volatile.Read(ref _state).Index;

    public void Load(CatalogueSnapshot snapshot) =>
        Volatile.Write(ref _state, new State(snapshot, SearchIndex.Build(snapshot)));

    public TrackRef? FindTrack(string trackId)
    {
        var snapshot = Snapshot;
        if (!snapshot.Tracks.TryGetValue(trackId, out var track))
        {
            return null;
        }

        return new TrackRef(track.Id, track.Title, snapshot.Artists[track.ArtistId].LabelId, track.DurationSeconds);
    }

    public LabelRef? FindLabel(string labelId) =>
        Snapshot.Labels.TryGetValue(labelId, out var label) ? new LabelRef(label.Id, label.Name) : null;

    public IReadOnlyList<LabelRef> Labels() =>
        Snapshot.Labels.Values.OrderBy(l => l.Name, StringComparer.Ordinal).Select(l => new LabelRef(l.Id, l.Name)).ToList();

    private sealed record State(CatalogueSnapshot Snapshot, SearchIndex Index);
}
