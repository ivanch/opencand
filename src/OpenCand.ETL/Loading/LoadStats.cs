using System.Collections.Concurrent;

namespace OpenCand.ETL.Loading
{
    public class EtlException : Exception
    {
        public EtlException(string message, Exception? inner = null) : base(message, inner) { }
    }

    /// <summary>Thread-safe counters for one dataset (all its files).</summary>
    public sealed class LoadStats
    {
        private long read;
        private long written;
        private readonly ConcurrentDictionary<string, long> rejected = new();
        private readonly ConcurrentDictionary<string, long> warnings = new();

        public LoadStats(string dataset)
        {
            Dataset = dataset;
        }

        public string Dataset { get; }
        public int Files { get; set; }
        public long Read => Interlocked.Read(ref read);
        public long Written => Interlocked.Read(ref written);
        public long RejectedTotal => rejected.Values.Sum();
        public IReadOnlyDictionary<string, long> Rejected => rejected;
        public IReadOnlyDictionary<string, long> Warnings => warnings;

        public void AddRead() => Interlocked.Increment(ref read);
        public void AddWritten() => Interlocked.Increment(ref written);
        public void Reject(string reason) => rejected.AddOrUpdate(reason, 1, (_, count) => count + 1);
        public void Warn(string reason) => warnings.AddOrUpdate(reason, 1, (_, count) => count + 1);

        public double RejectRatio => Read == 0 ? 0 : (double)RejectedTotal / Read;

        public void EnsureWithin(double maxRatio)
        {
            if (RejectRatio > maxRatio)
            {
                throw new EtlException(
                    $"{Dataset}: {RejectedTotal} of {Read} rows were rejected ({RejectRatio:P2}), above the allowed {maxRatio:P2}. " +
                    $"Reasons: {Describe(rejected)}");
            }
        }

        public string Summary()
        {
            var text = $"{Dataset}: files={Files} read={Read} written={Written} rejected={RejectedTotal}";
            if (!rejected.IsEmpty) text += $" [{Describe(rejected)}]";
            if (!warnings.IsEmpty) text += $" warnings [{Describe(warnings)}]";
            return text;
        }

        private static string Describe(IEnumerable<KeyValuePair<string, long>> counts)
            => string.Join(", ", counts.OrderByDescending(c => c.Value).Select(c => $"{c.Key}={c.Value}"));
    }
}
