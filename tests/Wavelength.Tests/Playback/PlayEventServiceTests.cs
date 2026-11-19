using Wavelength.Playback.Application;
using Wavelength.Playback.Contracts;
using Wavelength.SharedKernel;
using Wavelength.Tests.Support;
using Xunit;

namespace Wavelength.Playback.Tests;

public class PlayEventServiceTests
{
    private readonly CapturingPublisher _publisher = new();
    private readonly ManualTimeProvider _clock = new(new DateTimeOffset(2026, 11, 10, 8, 0, 0, TimeSpan.Zero));
    private readonly PlayEventService _service;

    public PlayEventServiceTests()
    {
        _service = new PlayEventService(new FakeDirectory(), _publisher, _clock, new InMemoryPlaySessionStore());
    }

    private static PlayEventRequest Request(Guid playId, string trackId, int seconds, string reason = "threshold") =>
        new(playId.ToString(), trackId, seconds, reason);

    [Fact]
    public void A_31_second_play_publishes_one_qualified_play_for_the_tracks_label()
    {
        var playId = Guid.NewGuid();

        var outcome = _service.Record(Request(playId, "trk-a", 31));

        Assert.Equal(PlayEventStatus.Accepted, outcome.Status);
        Assert.True(outcome.Response!.CountedNow);
        var evt = Assert.Single(_publisher.Published) as TrackPlayQualified;
        Assert.NotNull(evt);
        Assert.Equal(playId, evt!.PlayId);
        Assert.Equal("lbl-one", evt.LabelId);
        Assert.Equal(_clock.GetUtcNow(), evt.QualifiedAt);
    }

    [Fact]
    public void The_same_play_event_sent_twice_publishes_once()
    {
        var playId = Guid.NewGuid();

        var first = _service.Record(Request(playId, "trk-a", 31));
        var second = _service.Record(Request(playId, "trk-a", 31));

        Assert.True(first.Response!.CountedNow);
        Assert.False(second.Response!.CountedNow);
        Assert.True(second.Response.Counted);
        Assert.Single(_publisher.Published);
    }

    [Fact]
    public void A_10_second_play_publishes_nothing()
    {
        var outcome = _service.Record(Request(Guid.NewGuid(), "trk-a", 10, "skipped"));

        Assert.Equal(PlayEventStatus.Accepted, outcome.Status);
        Assert.False(outcome.Response!.Counted);
        Assert.Empty(_publisher.Published);
    }

    [Fact]
    public void Threshold_then_end_report_for_one_play_counts_once()
    {
        var playId = Guid.NewGuid();

        _service.Record(Request(playId, "trk-a", 30));
        _service.Record(Request(playId, "trk-a", 200, "ended"));

        Assert.Single(_publisher.Published);
    }

    [Fact]
    public void A_track_shorter_than_30_seconds_never_counts()
    {
        var outcome = _service.Record(Request(Guid.NewGuid(), "trk-short", 20, "ended"));

        Assert.False(outcome.Response!.Counted);
        Assert.Empty(_publisher.Published);
    }

    [Fact]
    public void Unknown_track_is_not_found()
    {
        var outcome = _service.Record(Request(Guid.NewGuid(), "trk-nope", 31));

        Assert.Equal(PlayEventStatus.TrackNotFound, outcome.Status);
        Assert.Empty(_publisher.Published);
    }

    [Theory]
    [InlineData("not-a-guid", "trk-a", 31, "threshold", "playId")]
    [InlineData("00000000-0000-0000-0000-000000000000", "trk-a", 31, "threshold", "playId")]
    [InlineData("8f0c1f8e-7a43-4c39-a7a3-1d2b54b0e001", "", 31, "threshold", "trackId")]
    [InlineData("8f0c1f8e-7a43-4c39-a7a3-1d2b54b0e001", "trk-a", -1, "threshold", "listenedSeconds")]
    [InlineData("8f0c1f8e-7a43-4c39-a7a3-1d2b54b0e001", "trk-a", 500, "threshold", "listenedSeconds")]
    [InlineData("8f0c1f8e-7a43-4c39-a7a3-1d2b54b0e001", "trk-a", 31, "rewound", "reason")]
    public void Invalid_events_are_rejected_with_field_errors(string playId, string trackId, int seconds, string reason, string field)
    {
        var outcome = _service.Record(new PlayEventRequest(playId, trackId, seconds, reason));

        Assert.Equal(PlayEventStatus.Invalid, outcome.Status);
        Assert.True(outcome.Errors!.ContainsKey(field), $"expected an error for {field}");
        Assert.Empty(_publisher.Published);
    }

    [Fact]
    public void Reusing_a_play_id_for_another_track_is_a_conflict()
    {
        var playId = Guid.NewGuid();
        _service.Record(Request(playId, "trk-a", 31));

        var outcome = _service.Record(Request(playId, "trk-b", 31));

        Assert.Equal(PlayEventStatus.Conflict, outcome.Status);
        Assert.Single(_publisher.Published);
    }

    [Fact]
    public void If_publishing_fails_the_play_can_be_counted_on_retry()
    {
        var failing = new FailOncePublisher();
        var service = new PlayEventService(new FakeDirectory(), failing, _clock, new InMemoryPlaySessionStore());
        var playId = Guid.NewGuid();

        Assert.Throws<InvalidOperationException>(() => service.Record(Request(playId, "trk-a", 31)));
        var retry = service.Record(Request(playId, "trk-a", 31));

        Assert.True(retry.Response!.CountedNow);
        Assert.Single(failing.Published);
    }

    private sealed class FailOncePublisher : IIntegrationEventPublisher
    {
        private bool _failed;

        public List<object> Published { get; } = [];

        public void Publish<TEvent>(TEvent integrationEvent)
            where TEvent : class
        {
            if (!_failed)
            {
                _failed = true;
                throw new InvalidOperationException("bus down");
            }

            Published.Add(integrationEvent);
        }
    }
}
