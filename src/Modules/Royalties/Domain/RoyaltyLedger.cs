using Wavelength.SharedKernel;

namespace Wavelength.Royalties.Domain;

/// <summary>One qualifying play, owed to a label. Never changed or removed once written.</summary>
internal sealed record LedgerEntry(long Sequence, Guid PlayId, string LabelId, string TrackId, DateTimeOffset QualifiedAt, Money Amount);

/// <summary>
/// Append-only royalty ledger. Each client play ID can be written once, so retries and duplicate events
/// can never pay a label twice. There is deliberately no update or delete.
/// </summary>
internal sealed class RoyaltyLedger
{
    private readonly object _gate = new();
    private readonly List<LedgerEntry> _entries = [];
    private readonly HashSet<Guid> _playIds = [];

    public int Count
    {
        get
        {
            lock (_gate)
            {
                return _entries.Count;
            }
        }
    }

    /// <summary>Appends the play, or returns null if this play ID is already in the ledger.</summary>
    public LedgerEntry? TryAppend(Guid playId, string labelId, string trackId, DateTimeOffset qualifiedAt, Money amount)
    {
        lock (_gate)
        {
            if (!_playIds.Add(playId))
            {
                return null;
            }

            var entry = new LedgerEntry(_entries.Count + 1, playId, labelId, trackId, qualifiedAt, amount);
            _entries.Add(entry);
            return entry;
        }
    }

    /// <summary>Entries for a label with <paramref name="fromInclusive"/> &lt;= QualifiedAt &lt; <paramref name="toExclusive"/>, in ledger order.</summary>
    public IReadOnlyList<LedgerEntry> Entries(string labelId, DateTimeOffset fromInclusive, DateTimeOffset toExclusive)
    {
        lock (_gate)
        {
            return _entries
                .Where(e => e.LabelId == labelId && e.QualifiedAt >= fromInclusive && e.QualifiedAt < toExclusive)
                .ToList();
        }
    }
}
