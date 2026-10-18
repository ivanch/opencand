using System.Globalization;
using OpenCand.ETL.Parsing;

namespace OpenCand.Tests.Etl;

public class TseParsingTests
{
    [Theory]
    [InlineData("1.234,56", "1234.56")]
    [InlineData("1234,56", "1234.56")]
    [InlineData("1234,5", "1234.5")]
    [InlineData("1000", "1000")]            // the old float/100 workaround turned this into 10.00
    [InlineData("1234.56", "1234.56")]      // single dot + 2 digits is a decimal point
    [InlineData("1.234", "1234")]           // dot + 3 digits are thousands
    [InlineData("1.234.567", "1234567")]
    [InlineData("2.000.000,00", "2000000.00")]
    [InlineData("-123,45", "-123.45")]
    [InlineData("R$ 10,00", "10.00")]
    [InlineData("0,00", "0.00")]
    public void TryParseMoney_ParsesBrazilianAndPlainNumbers(string raw, string expected)
    {
        Assert.True(TseParsing.TryParseMoney(raw, out var value));
        Assert.Equal(decimal.Parse(expected, CultureInfo.InvariantCulture), value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("#NULO")]
    [InlineData("#NE")]
    [InlineData("-4")]
    public void TryParseMoney_EmptyAndMarkersGiveNullAndSucceed(string? raw)
    {
        Assert.True(TseParsing.TryParseMoney(raw, out var value));
        Assert.Null(value);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("1,2,3")]
    [InlineData("12x")]
    public void TryParseMoney_GarbageFails(string raw)
    {
        Assert.False(TseParsing.TryParseMoney(raw, out _));
    }

    [Fact]
    public void TryParseMoney_DoesNotDependOnTheCurrentCulture()
    {
        var original = CultureInfo.CurrentCulture;
        try
        {
            foreach (var name in new[] { "pt-BR", "en-US", "de-DE", "" })
            {
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(name);
                Assert.True(TseParsing.TryParseMoney("1.234,56", out var value));
                Assert.Equal(1234.56m, value);
            }
        }
        finally
        {
            CultureInfo.CurrentCulture = original;
        }
    }

    [Theory]
    [InlineData("13/09/2022", 2022, 9, 13)]    // day > 12: the old culture-dependent parse read this as MM/dd and failed
    [InlineData("05/10/2022", 2022, 10, 5)]    // ambiguous day/month: must be dd/MM
    [InlineData("2022-10-05", 2022, 10, 5)]
    public void ParseDateTimeUtc_IsDayMonthYear(string raw, int year, int month, int day)
    {
        var parsed = TseParsing.ParseDateTimeUtc(raw);

        Assert.NotNull(parsed);
        Assert.Equal(new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Utc), parsed);
        Assert.Equal(DateTimeKind.Utc, parsed!.Value.Kind);
    }

    [Fact]
    public void ParseDateTimeUtc_KeepsTheTimeOfDay()
    {
        Assert.Equal(new DateTime(2022, 10, 5, 10, 30, 15, DateTimeKind.Utc), TseParsing.ParseDateTimeUtc("05/10/2022 10:30:15"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("#NULO")]
    [InlineData("31/02/2022")]
    [InlineData("not a date")]
    public void ParseDateTimeUtc_InvalidGivesNull(string? raw)
    {
        Assert.Null(TseParsing.ParseDateTimeUtc(raw));
    }

    [Fact]
    public void ParseDate_ParsesBirthDates()
    {
        Assert.Equal(new DateOnly(1970, 3, 5), TseParsing.ParseDate("05/03/1970"));
        Assert.Null(TseParsing.ParseDate("#NULO"));
    }

    [Theory]
    [InlineData("12345678909", "12345678909")]
    [InlineData("123.456.789-09", "12345678909")]
    [InlineData("#NULO#", null)]
    [InlineData("-4", null)]
    [InlineData("", null)]
    [InlineData("123", null)]
    [InlineData("1234567890123", null)]
    [InlineData("00000000000", null)]       // would merge unrelated people under the unique CPF index
    [InlineData("11111111111", null)]
    [InlineData("***.456.789-**", null)]
    public void ParseCpf(string? raw, string? expected)
    {
        Assert.Equal(expected, TseParsing.ParseCpf(raw));
    }

    [Theory]
    [InlineData("12345678909", "12345678909", null)]
    [InlineData("12345678000190", null, "12345678000190")]
    [InlineData("12.345.678/0001-90", null, "12345678000190")]
    [InlineData("-4", null, null)]
    [InlineData("", null, null)]
    [InlineData("12345", null, null)]
    public void SplitCpfCnpj(string raw, string? cpf, string? cnpj)
    {
        Assert.Equal((cpf, cnpj), TseParsing.SplitCpfCnpj(raw));
    }

    [Theory]
    [InlineData("facebook.com/maria", "facebook.com/maria")]
    [InlineData("https://x.com/maria", "https://x.com/maria")]
    [InlineData("http://www.instagram.com/maria", "http://www.instagram.com/maria")]
    public void NormalizeUrl_AcceptsHttpLinks(string raw, string expectedTail)
    {
        var url = TseParsing.NormalizeUrl(raw);

        Assert.NotNull(url);
        Assert.StartsWith("http", url);
        Assert.EndsWith(expectedTail, url);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("#NULO")]
    [InlineData("mailto:x@y.com")]
    [InlineData("javascript:alert(1)")]
    [InlineData("ftp://files.example.com/a")]
    [InlineData("@maria")]
    [InlineData("localhost")]
    [InlineData("not a url")]
    [InlineData("https://user:pass@evil.com/")]
    public void NormalizeUrl_RejectsEverythingElse(string? raw)
    {
        Assert.Null(TseParsing.NormalizeUrl(raw));
    }

    [Theory]
    [InlineData("https://www.facebook.com/a", "Facebook")]
    [InlineData("https://m.facebook.com/a", "Facebook")]
    [InlineData("https://twitter.com/a", "X/Twitter")]
    [InlineData("https://x.com/a", "X/Twitter")]
    [InlineData("https://mobile.x.com/a", "X/Twitter")]
    [InlineData("https://instagram.com/a", "Instagram")]
    [InlineData("https://youtu.be/a", "YouTube")]
    [InlineData("https://t.me/a", "Telegram")]
    [InlineData("https://wa.me/55", "WhatsApp")]
    [InlineData("https://api.whatsapp.com/send", "WhatsApp")]
    [InlineData("https://www.threads.net/@a", "Threads")]
    [InlineData("https://fox.com/a", "Outros")]            // "x.com" is a substring of "fox.com"
    [InlineData("https://netflix.com/a", "Outros")]
    [InlineData("https://notfacebook.com/a", "Outros")]
    [InlineData("https://example.com/facebook.com", "Outros")]
    public void ClassifyRede_MatchesTheHostNotASubstring(string url, string expected)
    {
        Assert.Equal(expected, TseParsing.ClassifyRede(url));
    }

    [Fact]
    public void NullIfTseEmpty_MapsMarkersAndBlanks()
    {
        Assert.Null(TseParsing.NullIfTseEmpty("  "));
        Assert.Null(TseParsing.NullIfTseEmpty("#NULO#"));
        Assert.Null(TseParsing.NullIfTseEmpty("NÃO DIVULGÁVEL"));
        Assert.Equal("MARIA", TseParsing.NullIfTseEmpty("  MARIA "));
    }
}
