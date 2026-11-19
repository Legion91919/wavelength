using Wavelength.Catalogue.Contracts;
using Wavelength.Catalogue.Domain;

namespace Wavelength.Catalogue.Infrastructure;

internal static class Mapping
{
    public const string CdnAudioPath = "/cdn/audio/";

    public static TrackDto ToDto(this Track t, CatalogueSnapshot s)
    {
        var artist = s.Artists[t.ArtistId];
        var label = s.LabelOf(artist);
        return new TrackDto(t.Id, t.Title, artist.Id, artist.Name, t.AlbumId, s.Albums[t.AlbumId].Title, label.Id, label.Name, t.DurationSeconds, CdnAudioPath + t.AudioFile);
    }

    public static ArtistDto ToDto(this Artist a, CatalogueSnapshot s)
    {
        var label = s.LabelOf(a);
        return new ArtistDto(a.Id, a.Name, label.Id, label.Name, s.TracksOfArtist(a.Id).Count());
    }

    public static AlbumDto ToDto(this Album a, CatalogueSnapshot s)
    {
        var artist = s.Artists[a.ArtistId];
        return new AlbumDto(a.Id, a.Title, artist.Id, artist.Name, s.LabelOf(artist).Name, a.Year, s.TracksOfAlbum(a.Id).Count());
    }
}
