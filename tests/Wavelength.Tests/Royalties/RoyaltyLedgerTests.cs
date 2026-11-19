using Wavelength.Royalties.Domain;
using Wavelength.SharedKernel;
using Xunit;

namespace Wavelength.Royalties.Tests;

public class RoyaltyLedgerTests
{
    private static readonly DateTimeOffset At = new(2026, 11, 10, 8, 0, 0, TimeSpan.Zero);
    private static readonly Money Rate = new(0.05m, "ZAR");

    [Fact]
    public void Appending_a_qualified_play_adds_one_entry_with_the_next_sequence()
    {
        var ledger = new RoyaltyLedger();

        var first = ledger.TryAppend(Guid.NewGuid(), "lbl-one", "trk-a", At, Rate);
        var second = ledger.TryAppend(Guid.NewGuid(), "lbl-one", "trk-b", At, Rate);

        Assert.NotNull(first);
        Assert.Equal(1, first!.Sequence);
        Assert.Equal(2, second!.Sequence);
        Assert.Equal(2, ledger.Count);
    }

    [Fact]
    public void The_same_play_id_is_appended_only_once()
    {
        var ledger = new RoyaltyLedger();
        var playId = Guid.NewGuid();

        Assert.NotNull(ledger.TryAppend(playId, "lbl-one", "trk-a", At, Rate));
        Assert.Null(ledger.TryAppend(playId, "lbl-one", "trk-a", At.AddSeconds(3), Rate));

        Assert.Equal(1, ledger.Count);
    }

    [Fact]
    public void Entries_are_filtered_by_label_and_half_open_period()
    {
        var ledger = new RoyaltyLedger();
        var from = new DateTimeOffset(2026, 11, 1, 0, 0, 0, TimeSpan.Zero);
        var to = new DateTimeOffset(2026, 12, 1, 0, 0, 0, TimeSpan.Zero);
        ledger.TryAppend(Guid.NewGuid(), "lbl-one", "trk-a", from, Rate);
        ledger.TryAppend(Guid.NewGuid(), "lbl-one", "trk-a", to.AddTicks(-1), Rate);
        ledger.TryAppend(Guid.NewGuid(), "lbl-one", "trk-a", to, Rate);
        ledger.TryAppend(Guid.NewGuid(), "lbl-two", "trk-c", At, Rate);

        var entries = ledger.Entries("lbl-one", from, to);

        Assert.Equal(2, entries.Count);
        Assert.All(entries, e => Assert.Equal("lbl-one", e.LabelId));
    }

    [Fact]
    public void Ledger_entries_cannot_be_changed_after_append()
    {
        var mutable = typeof(LedgerEntry).GetProperties().Where(p => p.SetMethod is { IsPublic: true } && !p.SetMethod.ReturnParameter.GetRequiredCustomModifiers().Any(m => m.Name == "IsExternalInit"));

        Assert.Empty(mutable);
        Assert.DoesNotContain(typeof(RoyaltyLedger).GetMethods(), m => m.Name.StartsWith("Remove", StringComparison.Ordinal) || m.Name.StartsWith("Update", StringComparison.Ordinal));
    }

    [Fact]
    public void Concurrent_duplicate_appends_still_record_one_entry()
    {
        var ledger = new RoyaltyLedger();
        var playId = Guid.NewGuid();

        Parallel.For(0, 64, _ => ledger.TryAppend(playId, "lbl-one", "trk-a", At, Rate));

        Assert.Equal(1, ledger.Count);
    }
}
