using System.Collections.Concurrent;

namespace OpenCand.API.Services
{
    /// <summary>
    /// In-memory counter of candidate page views. <see cref="PopularityFlushService"/> persists it periodically,
    /// so a page view does not cost a database write.
    /// </summary>
    public class PopularityTracker
    {
        private readonly ConcurrentDictionary<Guid, int> counts = new();

        public int Pending => counts.Count;

        public void Increment(Guid idcandidato) => counts.AddOrUpdate(idcandidato, 1, (_, current) => current + 1);

        /// <summary>Removes and returns everything counted so far.</summary>
        public (Guid[] Ids, int[] Counts) Drain()
        {
            var ids = new List<Guid>();
            var values = new List<int>();

            foreach (var id in counts.Keys)
            {
                if (counts.TryRemove(id, out var value))
                {
                    ids.Add(id);
                    values.Add(value);
                }
            }

            return (ids.ToArray(), values.ToArray());
        }

        /// <summary>Puts counts back after a failed flush so they are retried on the next one.</summary>
        public void Restore(Guid[] ids, int[] values)
        {
            for (var i = 0; i < ids.Length; i++)
            {
                counts.AddOrUpdate(ids[i], values[i], (_, current) => current + values[i]);
            }
        }
    }
}
