using OpenCand.API.Services;

namespace OpenCand.Tests.Api;

public class PopularityTrackerTests
{
    [Fact]
    public void Drain_ReturnsCountsPerCandidateAndEmptiesTheTracker()
    {
        var tracker = new PopularityTracker();
        var a = Guid.NewGuid();
        var b = Guid.NewGuid();

        tracker.Increment(a);
        tracker.Increment(a);
        tracker.Increment(b);

        var (ids, counts) = tracker.Drain();

        Assert.Equal(2, ids.Length);
        Assert.Equal(2, counts[Array.IndexOf(ids, a)]);
        Assert.Equal(1, counts[Array.IndexOf(ids, b)]);
        Assert.Equal(0, tracker.Pending);
        Assert.Empty(tracker.Drain().Ids);
    }

    [Fact]
    public void Restore_MergesWithViewsCountedMeanwhile()
    {
        var tracker = new PopularityTracker();
        var a = Guid.NewGuid();
        tracker.Increment(a);
        var (ids, counts) = tracker.Drain();

        tracker.Increment(a); // view arrives while the flush is failing
        tracker.Restore(ids, counts);

        var (_, after) = tracker.Drain();
        Assert.Equal(new[] { 2 }, after);
    }

    [Fact]
    public async Task Increment_IsThreadSafe()
    {
        var tracker = new PopularityTracker();
        var id = Guid.NewGuid();

        await Task.WhenAll(Enumerable.Range(0, 8).Select(_ => Task.Run(() =>
        {
            for (var i = 0; i < 1000; i++) tracker.Increment(id);
        })));

        Assert.Equal(new[] { 8000 }, tracker.Drain().Counts);
    }
}
