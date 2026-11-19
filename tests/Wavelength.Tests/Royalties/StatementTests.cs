using Wavelength.Royalties.Application;
using Wavelength.Royalties.Domain;
using Wavelength.SharedKernel;
using Wavelength.Tests.Support;
using Xunit;

namespace Wavelength.Royalties.Tests;

public class StatementTests
{
    private static readonly RoyaltyOptions Options = new() { RatePerPlay = 0.05m, Currency = "ZAR" };
    private readonly RoyaltyLedger _ledger = new();
    private readonly StatementService _service;
    private readonly QualifiedPlayHandler _handler;

    public StatementTests()
    {
        _service = new StatementService(_ledger, new FakeDirectory());
        _handler = new QualifiedPlayHandler(_ledger, Microsoft.Extensions.Options.Options.Create(Options));
    }

    private void Qualify(string trackId, string labelId, DateTimeOffset at) =>
        _handler.Handle(new TrackPlayQualified(Guid.NewGuid(), trackId, labelId, at));

    private static DateTimeOffset Nov(int day, int hour = 12) => new(2026, 11, day, hour, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Statement_sums_qualifying_plays_per_track_at_the_rate()
    {
        Qualify("trk-a", "lbl-one", Nov(2));
        Qualify("trk-a", "lbl-one", Nov(3));
        Qualify("trk-b", "lbl-one", Nov(4));

        var statement = _service.Build("lbl-one", StatementPeriod.Create(new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 30)).Value!)!;

        Assert.Equal(3, statement.TotalPlays);
        Assert.Equal(0.15m, statement.TotalAmount);
        Assert.Equal("ZAR", statement.Currency);
        var a = Assert.Single(statement.Lines, l => l.TrackId == "trk-a");
        Assert.Equal(2, a.Plays);
        Assert.Equal(0.10m, a.Amount);
        Assert.Equal("Test Track A", a.TrackTitle);
    }

    [Fact]
    public void Statement_reconciles_exactly_with_the_ledger()
    {
        for (var i = 0; i < 37; i++)
        {
            Qualify(i % 2 == 0 ? "trk-a" : "trk-b", "lbl-one", Nov(1 + (i % 28)));
        }

        var period = StatementPeriod.Create(new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 30)).Value!;
        var statement = _service.Build("lbl-one", period)!;
        var ledgerTotal = _ledger.Entries("lbl-one", period.StartsAt, period.EndsBefore).Sum(e => e.Amount.Amount);

        Assert.Equal(37, statement.Reconciliation.LedgerEntryCount);
        Assert.Equal(ledgerTotal, statement.Reconciliation.LedgerTotal);
        Assert.Equal(statement.TotalAmount, statement.Reconciliation.LedgerTotal);
        Assert.Equal(statement.TotalPlays, statement.Lines.Sum(l => l.Plays));
        Assert.Equal(statement.TotalAmount, statement.Lines.Sum(l => l.Amount));
        Assert.True(statement.Reconciliation.Reconciled);
    }

    [Fact]
    public void Statement_excludes_other_labels_and_plays_outside_the_period()
    {
        Qualify("trk-a", "lbl-one", Nov(30, 23));
        Qualify("trk-a", "lbl-one", new DateTimeOffset(2026, 12, 1, 0, 0, 0, TimeSpan.Zero));
        Qualify("trk-a", "lbl-one", new DateTimeOffset(2026, 10, 31, 23, 59, 59, TimeSpan.Zero));
        Qualify("trk-c", "lbl-two", Nov(5));

        var statement = _service.Build("lbl-one", StatementPeriod.Create(new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 30)).Value!)!;

        Assert.Equal(1, statement.TotalPlays);
    }

    [Fact]
    public void Duplicate_qualified_events_are_counted_once()
    {
        var evt = new TrackPlayQualified(Guid.NewGuid(), "trk-a", "lbl-one", Nov(2));
        _handler.Handle(evt);
        _handler.Handle(evt);

        var statement = _service.Build("lbl-one", StatementPeriod.Create(new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 30)).Value!)!;

        Assert.Equal(1, statement.TotalPlays);
    }

    [Fact]
    public void Empty_period_gives_a_zero_statement_that_still_reconciles()
    {
        var statement = _service.Build("lbl-two", StatementPeriod.Create(new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 30)).Value!)!;

        Assert.Equal(0, statement.TotalPlays);
        Assert.Equal(0m, statement.TotalAmount);
        Assert.Empty(statement.Lines);
        Assert.True(statement.Reconciliation.Reconciled);
    }

    [Fact]
    public void Unknown_label_has_no_statement()
    {
        Assert.Null(_service.Build("lbl-nope", StatementPeriod.Create(new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 30)).Value!));
    }

    [Theory]
    [InlineData(2026, 11, 30, 2026, 11, 1)]
    [InlineData(2025, 1, 1, 2026, 6, 1)]
    public void Invalid_periods_are_rejected(int fy, int fm, int fd, int ty, int tm, int td)
    {
        var result = StatementPeriod.Create(new DateOnly(fy, fm, fd), new DateOnly(ty, tm, td));

        Assert.Null(result.Value);
        Assert.NotNull(result.Error);
    }

    [Fact]
    public void Money_does_not_mix_currencies()
    {
        Assert.Throws<InvalidOperationException>(() => _ = new Money(1m, "ZAR") + new Money(1m, "USD"));
    }
}
