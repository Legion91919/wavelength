namespace Wavelength.Royalties.Domain;

internal sealed record PeriodResult(StatementPeriod? Value, string? Error);

/// <summary>A statement period of whole UTC days, both ends inclusive, at most 366 days long.</summary>
internal sealed record StatementPeriod(DateOnly From, DateOnly To)
{
    public const int MaxDays = 366;

    public DateTimeOffset StartsAt => new(From.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);

    public DateTimeOffset EndsBefore => new(To.AddDays(1).ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);

    public static PeriodResult Create(DateOnly from, DateOnly to)
    {
        if (to < from)
        {
            return new PeriodResult(null, "'to' must be on or after 'from'.");
        }

        if (to.DayNumber - from.DayNumber + 1 > MaxDays)
        {
            return new PeriodResult(null, $"A statement covers at most {MaxDays} days.");
        }

        return new PeriodResult(new StatementPeriod(from, to), null);
    }
}
