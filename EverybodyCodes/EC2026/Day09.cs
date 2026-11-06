using System.Text.RegularExpressions;
using Challenge.Collections;
using Challenge.Solvers;
using Challenge.Solvers.Specialized;
using Challenge.Utils;

namespace EverybodyCodes.EC2026;

/// <summary>
/// Solver for 2026 Day 9
/// </summary>
[Solver(2026, 9)]
public sealed partial class Day09 : RegexSolver<Day09.Plant>, IEverybodyCodesSolver
{
    public readonly record struct Plant(int ID, long Growth, long Limit);

    /// <inheritdoc />
    [GeneratedRegex(@"id=(\d+), growth=(\d+), limit=(\d+)")]
    protected override partial Regex Matcher { get; }

    // ReSharper disable CognitiveComplexity
    /// <inheritdoc />
    [Part(1)]
    public void RunPart1()
    {
        int totalTime = 0;
        Counter<long, int> branches     = new(1000);
        Counter<long, int> nextBranches = new(1000);
        foreach (Plant plant in this.Data)
        {
            int time     = 1;
            int length   = 1;
            branches[1L] = 1;
            while (length < plant.Limit)
            {
                foreach ((long size, int count) in branches)
                {
                    nextBranches[size + 1L] = count;
                    length += count;
                }

                int splitCount = nextBranches[plant.Growth];
                nextBranches[plant.Growth / 2L] += splitCount * 2;
                nextBranches[1L] += splitCount;
                length += splitCount;
                time++;
                nextBranches.RemoveAll(plant.Growth);

                SwapUtils.Swap(ref branches, ref nextBranches);
                nextBranches.Clear();
            }

            totalTime += time;
            branches.Clear();
        }

        LogAnswer(totalTime);
    }

    /// <inheritdoc />
    [Part(2)]
    public void RunPart2()
    {
        long totalTime = 0L;
        Counter<long, long> branches     = new(1000);
        Counter<long, long> nextBranches = new(1000);
        foreach (Plant plant in this.Data)
        {
            long time    = 1L;
            long leaves  = 1L;
            branches[1L] = 1L;
            while (leaves < plant.Limit)
            {
                long skip = plant.Growth - branches.Keys.Max();
                foreach ((long size, long count) in branches)
                {
                    nextBranches[size + skip] = count;
                }

                long splitCount = nextBranches[plant.Growth];
                nextBranches[plant.Growth / 2L] += splitCount * 2L;
                nextBranches[1L] += splitCount;
                leaves += splitCount;
                time   += skip;
                nextBranches.RemoveAll(plant.Growth);

                SwapUtils.Swap(ref branches, ref nextBranches);
                nextBranches.Clear();
            }

            totalTime += time;
            branches.Clear();
        }

        LogAnswer(totalTime);
    }

    /// <inheritdoc />
    [Part(3)]
    public void RunPart3() => RunPart2();
    // ReSharper enable CognitiveComplexity
}
