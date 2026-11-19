namespace Wavelength.Royalties.Contracts;

public sealed record StatementLine(string TrackId, string TrackTitle, int Plays, decimal Amount);

/// <summary>Independent totals straight from the ledger. <c>Reconciled</c> is true only when they equal the statement to the cent.</summary>
public sealed record StatementReconciliation(int LedgerEntryCount, decimal LedgerTotal, long? FirstSequence, long? LastSequence, bool Reconciled);

public sealed record LabelStatementResponse(
    string LabelId,
    string LabelName,
    DateOnly From,
    DateOnly To,
    string Currency,
    IReadOnlyList<StatementLine> Lines,
    int TotalPlays,
    decimal TotalAmount,
    StatementReconciliation Reconciliation);

public sealed record LedgerEntryResponse(long Sequence, Guid PlayId, string TrackId, DateTimeOffset QualifiedAt, decimal Amount, string Currency);
