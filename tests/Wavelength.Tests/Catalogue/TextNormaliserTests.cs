using Wavelength.Catalogue.Search;
using Xunit;

namespace Wavelength.Catalogue.Tests;

public class TextNormaliserTests
{
    [Theory]
    [InlineData("Méridien", "meridien")]
    [InlineData("ŌMBRE Coast", "ombre coast")]
    [InlineData("Thembé & The Tide", "thembe the tide")]
    [InlineData("  Café   Kwasa! ", "cafe kwasa")]
    [InlineData("Protéa in the Rain", "protea in the rain")]
    [InlineData("Søren's Fjørd", "sorens fjord")]
    public void Normalise_strips_accents_case_and_punctuation(string input, string expected)
    {
        Assert.Equal(expected, TextNormaliser.Normalise(input));
    }

    [Fact]
    public void Tokens_splits_normalised_text_into_words()
    {
        Assert.Equal(new[] { "night", "bus", "to", "meridian", "street" }, TextNormaliser.Tokens("Night Bus to Meridian-Street"));
    }

    [Fact]
    public void Tokens_of_blank_text_is_empty()
    {
        Assert.Empty(TextNormaliser.Tokens("  !!  "));
    }
}
