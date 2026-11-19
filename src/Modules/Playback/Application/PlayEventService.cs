using System.Collections.Concurrent;
using Wavelength.Playback.Contracts;
using Wavelength.Playback.Domain;
using Wavelength.SharedKernel;

namespace Wavelength.Playback.Application;

internal enum PlayEventStatus
{
    Accepted,
    Invalid,
    TrackNotFound,
    Conflict,
}

internal sealed record PlayEventOutcome(PlayEventStatus Status, PlayEventResponse? Response = null, Dictionary<string, string[]>? Errors = null, string? Detail = null);

internal interface IPlaySessionStore
{
    PlaySession GetOrAdd(Guid playId, Func<PlaySession> create);
}

internal sealed class InMemoryPlaySessionStore : IPlaySessionStore
{
    private readonly ConcurrentDictionary<Guid, PlaySession> _sessions = new();

    public PlaySession GetOrAdd(Guid playId, Func<PlaySession> create) => _sessions.GetOrAdd(playId, _ => create());
}

/// <summary>Records play reports idempotently and announces each qualifying play exactly once.</summary>
internal sealed class PlayEventService(ICatalogueDirectory directory, IIntegrationEventPublisher publisher, TimeProvider clock, IPlaySessionStore sessions)
{
    /// <summary>Allowance for clock drift between the audio element and the catalogue duration.</summary>
    public const int DurationGraceSeconds = 5;

    private static readonly string[] Reasons = ["threshold", "ended", "skipped"];

    public PlayEventOutcome Record(PlayEventRequest request)
    {
        var errors = new Dictionary<string, string[]>();
        if (!Guid.TryParse(request.PlayId, out var playId) || playId == Guid.Empty)
        {
            errors["playId"] = ["playId must be a client-generated, non-empty UUID."];
        }

        if (string.IsNullOrWhiteSpace(request.TrackId))
        {
            errors["trackId"] = ["trackId is required."];
        }

        if (request.ListenedSeconds is null or < 0)
        {
            errors["listenedSeconds"] = ["listenedSeconds must be zero or more."];
        }

        if (request.Reason is null || !Reasons.Contains(request.Reason))
        {
            errors["reason"] = ["reason must be one of: threshold, ended, skipped."];
        }

        if (errors.Count > 0)
        {
            return new PlayEventOutcome(PlayEventStatus.Invalid, Errors: errors);
        }

        var track = directory.FindTrack(request.TrackId!);
        if (track is null)
        {
            return new PlayEventOutcome(PlayEventStatus.TrackNotFound, Detail: $"No track with id '{request.TrackId}'.");
        }

        var listened = request.ListenedSeconds!.Value;
        if (listened > track.DurationSeconds + DurationGraceSeconds)
        {
            errors["listenedSeconds"] = [$"listenedSeconds cannot exceed the track length ({track.DurationSeconds} s)."];
            return new PlayEventOutcome(PlayEventStatus.Invalid, Errors: errors);
        }

        var session = sessions.GetOrAdd(playId, () => PlaySession.Start(playId, track.TrackId, track.LabelId));
        lock (session)
        {
            if (!string.Equals(session.TrackId, track.TrackId, StringComparison.Ordinal))
            {
                return new PlayEventOutcome(PlayEventStatus.Conflict, Detail: "This playId already belongs to another track. Generate a new playId per listen.");
            }

            var countedNow = session.RecordListened(listened, clock.GetUtcNow());
            if (countedNow)
            {
                try
                {
                    publisher.Publish(new TrackPlayQualified(session.PlayId, session.TrackId, session.LabelId, session.QualifiedAt!.Value));
                }
                catch
                {
                    session.RevertQualification();
                    throw;
                }
            }

            return new PlayEventOutcome(
                PlayEventStatus.Accepted,
                new PlayEventResponse(session.PlayId, session.TrackId, session.ListenedSeconds, session.Qualified, countedNow));
        }
    }
}
