using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Logging;
using OpenCand.ETL.Loading;

namespace OpenCand.ETL.Parsing
{
    public static partial class CsvSource
    {
        /// <summary>
        /// Lists the *.csv files of a dataset folder, ordered by the election year in the file name and then by name.
        /// Files with the "fix_" prefix are leftovers of the old ETL (which wrote repaired copies next to the input) and are skipped.
        /// </summary>
        public static List<string> ListFiles(string directory, ILogger logger)
        {
            var files = new List<string>();
            foreach (var path in Directory.GetFiles(directory, "*.csv"))
            {
                if (Path.GetFileName(path).StartsWith("fix_", StringComparison.OrdinalIgnoreCase))
                {
                    logger.LogInformation("Skipping legacy repaired file {File}", path);
                    continue;
                }

                files.Add(path);
            }

            return files
                .OrderBy(f => YearInName(Path.GetFileName(f)))
                .ThenBy(f => f, StringComparer.Ordinal)
                .ToList();
        }

        private static int YearInName(string fileName)
        {
            var match = YearRegex().Match(fileName);
            return match.Success ? int.Parse(match.Value) : 0;
        }

        /// <summary>
        /// Streams the records of a TSE CSV file: cp1252 decoding, broken-line repair, one record in memory at a time.
        /// Records that cannot be converted are counted as rejected ("csv_conversion") instead of aborting the file.
        /// </summary>
        public static IEnumerable<T> Read<T>(string path, Encoding encoding, LoadStats stats, ILogger logger)
        {
            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 1 << 16, FileOptions.SequentialScan);
            using var streamReader = new StreamReader(stream, encoding, detectEncodingFromByteOrderMarks: false, bufferSize: 1 << 16);
            using var repairing = new RepairingLineReader(streamReader);

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ";",
                HasHeaderRecord = true,
                PrepareHeaderForMatch = args => args.Header.ToLowerInvariant(),
                MissingFieldFound = null,
                TrimOptions = TrimOptions.Trim,
                // stray quotes inside unquoted fields are common in the TSE files; keep the text and count it
                BadDataFound = _ => stats.Warn("bad_data"),
                ReadingExceptionOccurred = args =>
                {
                    stats.AddRead();
                    stats.Reject("csv_conversion");
                    logger.LogDebug(args.Exception, "Could not convert a record of {File}", path);
                    return false; // skip the record, keep reading
                }
            };

            using var csv = new CsvReader(repairing, config);
            foreach (var record in csv.GetRecords<T>())
            {
                stats.AddRead();
                yield return record;
            }

            if (repairing.RepairedLines > 0)
            {
                logger.LogInformation("{File}: {Count} broken records were repaired", Path.GetFileName(path), repairing.RepairedLines);
            }
        }

        [GeneratedRegex(@"(?<!\d)(19|20)\d{2}(?!\d)")]
        private static partial Regex YearRegex();
    }
}
