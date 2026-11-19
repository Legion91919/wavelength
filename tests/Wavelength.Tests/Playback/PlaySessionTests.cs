using Wavelength.Playback.Domain;
using Xunit;

namespace Wavelength.Playback.Tests;

public class PlaySessionTests
{
    private static readonly DateTimeOffset At = new(2026, 11, 10, 8, 0, 0, TimeSpan.Zero);

    [Theory]
    [InlineData(0, false)]
    [InlineData(10, false)]
    [InlineData(29, false)]
    [InlineData(30, true)]
    [InlineData(31, true)]
    public void A_play_qualifies_from_30_listened_seconds(int seconds, bool qualifies)
    {
        Assert.Equal(qualifies, RoyaltyRule.Qualifies(seconds));
    }

    [Fact]
    public void A_play_of_31_seconds_qualifies_once()
    {
        var session = PlaySession.Start(Guid.NewGuid(), "trk-a", "lbl-one");

        Assert.True(session.RecordListened(31, At));
        Assert.True(session.Qualified);
        Assert.Equal(At, session.QualifiedAt);
    }

    [Fact]
    public void A_play_of_10_seconds_does_not_qualify()
    {
        var session = PlaySession.Start(Guid.NewGuid(), "trk-a", "lbl-one");

        Assert.False(session.RecordListened(10, At));
        Assert.False(session.Qualified);
        Assert.Null(session.QualifiedAt);
    }

    [Fact]
    public void Later_reports_for_the_same_play_never_qualify_again()
    {
        var session = PlaySession.Start(Guid.NewGuid(), "trk-a", "lbl-one");

        Assert.True(session.RecordListened(30, At));
        Assert.False(session.RecordListened(30, At.AddSeconds(5)));
        Assert.False(session.RecordListened(200, At.AddMinutes(3)));
        Assert.Equal(200, session.ListenedSeconds);
        Assert.Equal(At, session.QualifiedAt);
    }

    [Fact]
    public void Listened_time_never_goes_backwards()
    {
        var session = PlaySession.Start(Guid.NewGuid(), "trk-a", "lbl-one");

        session.RecordListened(12, At);
        session.RecordListened(5, At);

        Assert.Equal(12, session.ListenedSeconds);
    }

    [Fact]
    public void A_short_report_then_a_long_one_qualifies_on_the_long_one()
    {
        var session = PlaySession.Start(Guid.NewGuid(), "trk-a", "lbl-one");

        Assert.False(session.RecordListened(10, At));
        Assert.True(session.RecordListened(31, At.AddSeconds(21)));
    }
}
