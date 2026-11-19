namespace Wavelength.Catalogue.Contracts;

public sealed record TrackDto(
    string Id,
    string Title,
    string ArtistId,
    string ArtistName,
    string AlbumId,
    string AlbumTitle,
    string LabelId,
    string LabelName,
    int DurationSeconds,
    string AudioUrl);

public sealed record ArtistDto(string Id, string Name, string LabelId, string LabelName, int TrackCount);

public sealed record AlbumDto(string Id, string Title, string ArtistId, string ArtistName, string LabelName, int Year, int TrackCount);

public sealed record SearchResponse(string Query, IReadOnlyList<TrackDto> Tracks, IReadOnlyList<ArtistDto> Artists, IReadOnlyList<AlbumDto> Albums, double TookMs);

public sealed record ArtistDetailDto(ArtistDto Artist, IReadOnlyList<TrackDto> Tracks);

public sealed record AlbumDetailDto(AlbumDto Album, IReadOnlyList<TrackDto> Tracks);

public sealed record LabelDto(string Id, string Name);
