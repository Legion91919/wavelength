namespace Wavelength.Playback.Domain;

/// <summary>The royalty rule: a play counts once it has been listened to for 30 seconds.</summary>
internal static class RoyaltyRule
{
    public const int QualifyingSeconds = 30;

    public static bool Qualifies(int listenedSeconds) => listenedSeconds >= QualifyingSeconds;
}

/// <summary>
/// One listen of one track, identified by the client-generated play ID. Listened time only grows,
/// and the session qualifies for royalties at most once, however many reports arrive.
/// </summary>
internal sealed class PlaySession
{
    private PlaySession(Guid playId, string trackId, string labelId)
    {
        PlayId = playId;
        TrackId = trackId;
        LabelId = labelId;
    }

    public Guid PlayId { get; }

    public string TrackId { get; }

    public string LabelId { get; }

    public int ListenedSeconds { get; private set; }

    public DateTimeOffset? QualifiedAt { get; private set; }

    public bool Qualified => QualifiedAt is not null;

    public static PlaySession Start(Guid playId, string trackId, string labelId) => new(playId, trackId, labelId);

    /// <summary>Records a progress report. Returns true only on the report that first crosses the threshold.</summary>
    public bool RecordListened(int listenedSeconds, DateTimeOffset at)
    {
        if (listenedSeconds > ListenedSeconds)
        {
            ListenedSeconds = listenedSeconds;
        }

        if (Qualified || !RoyaltyRule.Qualifies(ListenedSeconds))
        {
            return false;
        }

        QualifiedAt = at;
        return true;
    }

    /// <summary>Used when the qualified event could not be handed on, so a client retry can count it.</summary>
    public void RevertQualification() => QualifiedAt = null;
}
