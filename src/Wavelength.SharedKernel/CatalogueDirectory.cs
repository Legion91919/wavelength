namespace Wavelength.SharedKernel;

/// <summary>What other modules may know about a track: enough to validate plays and attribute royalties.</summary>
public sealed record TrackRef(string TrackId, string Title, string LabelId, int DurationSeconds);

/// <summary>A label as other modules see it.</summary>
public sealed record LabelRef(string LabelId, string Name);

/// <summary>Read-only port implemented by the Catalogue module and consumed by Playback, Playlists and Royalties.</summary>
public interface ICatalogueDirectory
{
    TrackRef? FindTrack(string trackId);

    LabelRef? FindLabel(string labelId);

    IReadOnlyList<LabelRef> Labels();
}
