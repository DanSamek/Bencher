using System.Collections.Concurrent;
using Worker.ThreadSplitManager;

namespace Worker.Tests.ThreadSplitManager;

[TestFixture]
public class ThreadSplitTests
{
    public record SplitCase(int[] AvailableThreads, int MaxThreadsForTest, int PausedTests, int[] ExpectedSplit);

    private static SplitCase[] _splitCases = [
        new SplitCase([], 4, 5, []),
        new SplitCase([8], 4, 5, [8]),
        new SplitCase([16], 4, 5, [16]),
        new SplitCase([16], 2, 5, [16]),
        new SplitCase([16], 1, 5, [8, 8]),
        new SplitCase([32], 1, 5, [8, 8, 8, 8]),
        new SplitCase([32], 2, 5, [16, 16]),
        new SplitCase([64], 1, 5, [16, 16, 16, 16]),
        new SplitCase([64], 2, 5, [16, 16, 16, 16]),
        new SplitCase([64], 2, 1, [64]),
        new SplitCase([64], 2, 2, [32, 32]),
        new SplitCase([11], 2, 2, [11]),
        new SplitCase([11], 1, 2, [8, 3])
    ];
    
    /// <summary>
    /// Tests <see cref="Worker.ThreadSplitManager.ThreadSplit"/> 
    /// </summary>
    [TestCaseSource(nameof(_splitCases))]
    public void Split(SplitCase @case)
    {
        var queue = new ConcurrentQueue<int>(@case.AvailableThreads);
        var split = ThreadSplit.Split(queue, @case.MaxThreadsForTest, @case.PausedTests);
        Assert.That(split, Is.EquivalentTo(@case.ExpectedSplit));
    }
}