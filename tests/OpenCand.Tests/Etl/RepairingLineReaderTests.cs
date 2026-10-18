using OpenCand.ETL.Parsing;

namespace OpenCand.Tests.Etl;

public class RepairingLineReaderTests
{
    /// <summary>The algorithm of the old CsvFixerService, kept here as the reference ("golden") implementation.</summary>
    private static List<string>? OldFixer(string[] lines)
    {
        var headerCount = lines[0].Split(';').Length;
        var result = new List<string>();
        for (int i = 0; i < lines.Length;)
        {
            var line = lines[i];
            var columns = line.Split(';');
            var jump = 1;
            while (columns.Length != headerCount)
            {
                if (columns.Length > headerCount) break;
                if (i + jump >= lines.Length) return null; // the old fixer gave up on the whole file
                line += lines[i + jump];
                columns = line.Split(';');
                jump++;
            }

            result.Add(line);
            i += jump;
        }

        return result;
    }

    // same as File.ReadAllLines: a trailing line terminator does not add an empty last line
    private static string[] ReadAllLines(string text)
    {
        var lines = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None).ToList();
        if (lines.Count > 0 && lines[^1].Length == 0) lines.RemoveAt(lines.Count - 1);
        return lines.ToArray();
    }

    private static List<string> Repair(string text)
    {
        using var reader = new RepairingLineReader(new StringReader(text));
        var lines = new List<string>();
        string? line;
        while ((line = reader.ReadRepairedLine()) is not null) lines.Add(line);
        return lines;
    }

    public static IEnumerable<object[]> Cases()
    {
        yield return new object[] { "a;b;c\n1;2;3\n4;5;6\n" };
        yield return new object[] { "a;b;c\n1;2;3\n4;5\n;6\n7;8;9\n" };                  // one record broken over two lines
        yield return new object[] { "a;b;c\n1;2\n;\n3\n4;5;6\n" };                        // broken over four lines
        yield return new object[] { "a;b;c\n1;2;3;4\n5;6;7\n" };                          // more columns than the header: kept as-is
        yield return new object[] { "a;b;c\r\n1;2;3\r\n4;5\r\n;6\r\n" };                  // CRLF
        yield return new object[] { "\"a\";\"b\";\"c\"\n\"x\";\"y\n\";\"z\"\n" };            // newline right after a column separator inside a record
        yield return new object[] { "a;b\n1;2" };                                          // no trailing newline
    }

    [Theory]
    [MemberData(nameof(Cases))]
    public void ProducesTheSameRecordsAsTheOldFixer(string text)
    {
        var expected = OldFixer(ReadAllLines(text));

        Assert.NotNull(expected);
        Assert.Equal(expected, Repair(text));
    }

    [Fact]
    public void ReadsTheRepairedTextThroughTheTextReaderApi()
    {
        using var reader = new RepairingLineReader(new StringReader("a;b;c\n1;2\n;3\n4;5;6\n"));

        Assert.Equal("a;b;c\n1;2;3\n4;5;6\n", reader.ReadToEnd());
        Assert.Equal(1, reader.RepairedLines);
    }

    [Fact]
    public void ThrowsWhenTheFileEndsInsideABrokenRecord()
    {
        // the old fixer failed the whole file in this case; the stream must not silently drop the record
        using var reader = new RepairingLineReader(new StringReader("a;b;c\n1;2;3\n4;5\n"));

        Assert.Equal("a;b;c", reader.ReadRepairedLine());
        Assert.Equal("1;2;3", reader.ReadRepairedLine());
        Assert.Throws<InvalidDataException>(() => reader.ReadRepairedLine());
    }

    [Fact]
    public void WorksWithSmallReadBuffers()
    {
        using var reader = new RepairingLineReader(new StringReader("a;b;c\n1;2\n;3\n3;4;5\n"));
        var buffer = new char[3];
        var text = new System.Text.StringBuilder();
        int n;
        while ((n = reader.Read(buffer, 0, buffer.Length)) > 0) text.Append(buffer, 0, n);

        Assert.Equal("a;b;c\n1;2;3\n3;4;5\n", text.ToString());
    }
}
