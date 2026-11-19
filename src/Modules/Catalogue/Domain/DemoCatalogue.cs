namespace Wavelength.Catalogue.Domain;

/// <summary>Invented demo catalogue: every label, artist, album and track name is fictional.</summary>
internal static class DemoCatalogue
{
    private static readonly string[] Clips =
    [
        "wl-01-dusk-chord.mp3",
        "wl-02-harbour-fifths.mp3",
        "wl-03-lantern-pulse.mp3",
        "wl-04-salt-road-drone.mp3",
        "wl-05-kelp-arpeggio.mp3",
        "wl-06-signal-bells.mp3",
    ];

    public static CatalogueSnapshot Create()
    {
        Label[] labels =
        [
            new("lbl-jacaranda", "Jacaranda Tape Co."),
            new("lbl-ironveld", "Ironveld Records"),
            new("lbl-saltpan", "Saltpan Sounds"),
            new("lbl-bluekloof", "Blue Kloof Music"),
        ];
        Artist[] artists =
        [
            new("art-thembe", "Thembé & The Tide", "lbl-jacaranda"),
            new("art-fynbos", "Fynbos Arcade", "lbl-jacaranda"),
            new("art-kwasa", "Kwasa Lantern", "lbl-ironveld"),
            new("art-copper", "Copper Thorn Collective", "lbl-ironveld"),
            new("art-dune", "Dune Méridien", "lbl-saltpan"),
            new("art-lowveld", "Lowveld Radio Choir", "lbl-saltpan"),
            new("art-ombre", "Ōmbre Coast", "lbl-bluekloof"),
            new("art-protea", "Protéa Signal", "lbl-bluekloof"),
        ];
        Album[] albums =
        [
            new("alb-tidewater", "Tidewater Hymns", "art-thembe", 2026),
            new("alb-arcade", "Arcade in Bloom", "art-fynbos", 2025),
            new("alb-lantern", "Lantern Season", "art-kwasa", 2026),
            new("alb-copper", "Thorns & Wires", "art-copper", 2024),
            new("alb-meridien", "Méridien Nights", "art-dune", 2026),
            new("alb-longroad", "Choir of the Long Road", "art-lowveld", 2025),
            new("alb-salt", "Salt & Ember", "art-ombre", 2026),
            new("alb-lowtide", "Low Tide Merits", "art-ombre", 2025),
            new("alb-signal", "Signal Fires", "art-protea", 2026),
        ];
        (string Id, string Title, string Album, int Seconds)[] rows =
        [
            ("trk-tidewater-1", "Tidewater Hymn No. 1", "alb-tidewater", 204),
            ("trk-tidewater-2", "Tidewater Hymn No. 2", "alb-tidewater", 200),
            ("trk-harbour-light", "Harbour Light", "alb-tidewater", 187),
            ("trk-merino-morning", "Merino Morning", "alb-arcade", 72),
            ("trk-pinball-fynbos", "Pinball Fynbos", "alb-arcade", 158),
            ("trk-lantern-season", "Lantern Season", "alb-lantern", 284),
            ("trk-paraffin-glow", "Paraffin Glow", "alb-lantern", 233),
            ("trk-cafe-kwasa", "Café Kwasa", "alb-lantern", 176),
            ("trk-thorns-wires", "Thorns & Wires", "alb-copper", 241),
            ("trk-rust-rails", "Rust on the Rails", "alb-copper", 199),
            ("trk-night-bus", "Night Bus to Meridian Street", "alb-meridien", 221),
            ("trk-lanterns-saltpan", "Lanterns over Saltpan", "alb-meridien", 245),
            ("trk-salt-road", "Salt Road Lullaby", "alb-meridien", 178),
            ("trk-long-road", "Long Road Hymn", "alb-longroad", 312),
            ("trk-mbira-dusk", "Mbira at Dusk", "alb-longroad", 205),
            ("trk-salt-ember", "Salt & Ember", "alb-salt", 219),
            ("trk-ombre-tide", "Ōmbre Tide (Interlude)", "alb-salt", 48),
            ("trk-kelp-waltz", "Kelp Forest Waltz", "alb-salt", 263),
            ("trk-low-tide", "Low Tide Merits", "alb-lowtide", 192),
            ("trk-signal-fires", "Signal Fires", "alb-signal", 230),
            ("trk-protea-rain", "Protéa in the Rain", "alb-signal", 210),
            ("trk-fynbos-radio", "Fynbos Radio", "alb-signal", 166),
        ];

        var albumById = albums.ToDictionary(a => a.Id, StringComparer.Ordinal);
        var tracks = rows.Select((r, i) =>
        {
            var album = albumById[r.Album];
            var number = rows.Take(i).Count(x => x.Album == r.Album) + 1;
            return new Track(r.Id, r.Title, album.ArtistId, album.Id, number, r.Seconds, Clips[i % Clips.Length]);
        });

        return new CatalogueSnapshot(labels, artists, albums, tracks);
    }
}
