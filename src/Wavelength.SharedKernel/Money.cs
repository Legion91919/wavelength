using System.Globalization;

namespace Wavelength.SharedKernel;

/// <summary>An exact amount of money in one currency. Always decimal, never floating point.</summary>
public readonly record struct Money(decimal Amount, string Currency)
{
    public static Money Zero(string currency) => new(0m, currency);

    public static Money operator +(Money left, Money right)
    {
        if (!string.Equals(left.Currency, right.Currency, StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"Cannot add {right.Currency} to {left.Currency}.");
        }

        return new Money(left.Amount + right.Amount, left.Currency);
    }

    public static Money Add(Money left, Money right) => left + right;

    public override string ToString() => string.Create(CultureInfo.InvariantCulture, $"{Currency} {Amount:0.00##}");
}
