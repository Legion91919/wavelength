namespace Wavelength.Playlists.Contracts;

public sealed record PlaylistSummary(string Id, string Name, string Description, int TrackCount);

public sealed record PlaylistTrack(string TrackId, string Title, int DurationSeconds);

public sealed record PlaylistDetail(string Id, string Name, string Description, IReadOnlyList<PlaylistTrack> Tracks);
