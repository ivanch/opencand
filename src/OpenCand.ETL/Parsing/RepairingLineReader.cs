using System.Text;

namespace OpenCand.ETL.Parsing
{
    /// <summary>
    /// Streaming replacement for the old CsvFixerService. Some TSE files contain records broken over several physical
    /// lines. Like the old fixer, a line with fewer ';'-separated columns than the header is joined (without a
    /// separator) with the following lines until it has enough columns; lines with more columns are kept as-is.
    /// Nothing is written to disk and only one logical line is held in memory.
    /// </summary>
    public sealed class RepairingLineReader : TextReader
    {
        private readonly TextReader inner;
        private int headerColumns;
        private long physicalLine;
        private string? current;
        private int position;

        public RepairingLineReader(TextReader inner)
        {
            this.inner = inner;
        }

        /// <summary>Number of logical lines that had to be joined from several physical lines.</summary>
        public long RepairedLines { get; private set; }

        /// <summary>Returns the next repaired logical line (without terminator), or null at the end of the file.</summary>
        public string? ReadRepairedLine()
        {
            var line = inner.ReadLine();
            if (line is null) return null;
            physicalLine++;

            if (headerColumns == 0)
            {
                headerColumns = CountColumns(line);
                return line;
            }

            var columns = CountColumns(line);
            if (columns >= headerColumns) return line;

            var startLine = physicalLine;
            var builder = new StringBuilder(line);
            while (columns < headerColumns)
            {
                var next = inner.ReadLine();
                if (next is null)
                {
                    throw new InvalidDataException(
                        $"Reached the end of the file while repairing the record that starts at line {startLine} " +
                        $"({columns} columns, expected {headerColumns}).");
                }

                physicalLine++;
                builder.Append(next);
                // the last field of the current text and the first field of the next line become a single field
                columns += CountColumns(next) - 1;
            }

            RepairedLines++;
            return builder.ToString();
        }

        private static int CountColumns(string line) => line.AsSpan().Count(';') + 1;

        private bool Fill()
        {
            while (current is null || position >= current.Length)
            {
                var line = ReadRepairedLine();
                if (line is null)
                {
                    current = null;
                    return false;
                }

                current = line + "\n";
                position = 0;
            }

            return true;
        }

        public override int Peek() => Fill() ? current![position] : -1;

        public override int Read() => Fill() ? current![position++] : -1;

        public override int Read(char[] buffer, int index, int count)
        {
            if (!Fill()) return 0;

            var length = Math.Min(count, current!.Length - position);
            current.CopyTo(position, buffer, index, length);
            position += length;
            return length;
        }

        public override int Read(Span<char> buffer)
        {
            if (!Fill()) return 0;

            var length = Math.Min(buffer.Length, current!.Length - position);
            current.AsSpan(position, length).CopyTo(buffer);
            position += length;
            return length;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) inner.Dispose();
            base.Dispose(disposing);
        }
    }
}
