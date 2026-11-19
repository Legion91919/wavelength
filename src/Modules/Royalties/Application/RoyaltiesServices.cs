using Microsoft.Extensions.Options;
using Wavelength.Royalties.Contracts;
using Wavelength.Royalties.Domain;
using Wavelength.SharedKernel;

namespace Wavelength.Royalties.Application;

internal sealed class RoyaltyOptions
{
    public const string Section = "Royalties";

    /// <summary>Amount owed to the label per qualifying play.</summary>
    public decimal RatePerPlay { get; set; } = 0.05m;

    public string Currency { get; set; } = "ZAR";
}

/// <summary>Writes each qualifying play into the ledger. Safe to call twice with the same event.</summary>
internal sealed class QualifiedPlayHandler(RoyaltyLedger ledger, IOptions<RoyaltyOptions> options) : IIntegrationEventConsumer<TrackPlayQualified>
{
    public void Handle(TrackPlayQualified integrationEvent)
    {
        var rate = new Money(options.Value.RatePerPlay, options.Value.Currency);
        ledger.TryAppend(integrationEvent.PlayId, integrationEvent.LabelId, integrationEvent.TrackId, integrationEvent.QualifiedAt, rate);
    }
}

/// <summary>Builds a label statement from ledger entries and checks it against the ledger's own totals.</summary>
internal sealed class StatementService(RoyaltyLedger ledger, ICatalogueDirectory directory, IOptions<RoyaltyOptions>? options = null)
{
    public LabelStatementResponse? Build(string labelId, StatementPeriod period)
    {
        var label = directory.FindLabel(labelId);
        if (label is null)
        {
            return null;
        }

        var currency = options?.Value.Currency ?? "ZAR";
        var entries = ledger.Entries(labelId, period.StartsAt, period.EndsBefore);

        var lines = entries
            .GroupBy(e => e.TrackId, StringComparer.Ordinal)
            .Select(g => new StatementLine(
                g.Key,
                directory.FindTrack(g.Key)?.Title ?? g.Key,
                g.Count(),
                g.Aggregate(Money.Zero(currency), (sum, e) => sum + e.Amount).Amount))
            .OrderByDescending(l => l.Plays)
            .ThenBy(l => l.TrackTitle, StringComparer.Ordinal)
            .ToList();

        var totalPlays = lines.Sum(l => l.Plays);
        var totalAmount = lines.Aggregate(Money.Zero(currency), (sum, l) => sum + new Money(l.Amount, currency)).Amount;

        // Recomputed straight from the ledger, not from the lines, so a grouping bug cannot hide.
        var ledgerTotal = entries.Aggregate(Money.Zero(currency), (sum, e) => sum + e.Amount).Amount;
        var reconciliation = new StatementReconciliation(
            entries.Count,
            ledgerTotal,
            entries.Count > 0 ? entries[0].Sequence : null,
            entries.Count > 0 ? entries[^1].Sequence : null,
            entries.Count == totalPlays && ledgerTotal == totalAmount);

        return new LabelStatementResponse(label.LabelId, label.Name, period.From, period.To, currency, lines, totalPlays, totalAmount, reconciliation);
    }
}
