using System.Collections.Immutable;
using Challenge.Maths;
using Challenge.Solvers;
using Challenge.Solvers.Specialized;

namespace EverybodyCodes.EC2024;

/// <summary>
/// Solver for 2024 Day 9
/// </summary>
[Solver(2024, 9)]
public sealed partial class Day09 : ArraySolver<int>, IEverybodyCodesSolver
{
    private static readonly ImmutableArray<int> Part1Stamps = [1, 3, 5, 10];
    private static readonly ImmutableArray<int> Part2Stamps = [1, 3, 5, 10, 15, 16, 20, 24, 25, 30];
    private static readonly ImmutableArray<int> Part3Stamps = [1, 3, 5, 10, 15, 16, 20, 24, 25, 30, 37, 38, 49, 50, 74, 75, 100, 101];

    // ReSharper disable CognitiveComplexity
    /// <inheritdoc />
    [Part(1)]
    public void RunPart1()
    {
        int beetles = this.Data.Sum(s => MathUtils.MinimumCoinChange(s, Part1Stamps.AsSpan()).Length);
        LogAnswer(beetles);
    }

    /// <inheritdoc />
    [Part(2)]
    public void RunPart2()
    {
        int beetles = this.Data.Sum(s => MathUtils.MinimumCoinChange(s, Part2Stamps.AsSpan()).Length);
        LogAnswer(beetles);
    }

    /// <inheritdoc />
    [Part(3)]
    public void RunPart3()
    {
        int totalBeetles = 0;
        foreach (int sparkball in this.Data)
        {
            int minBeetles = int.MaxValue;
            for (int a = sparkball / 2, b = sparkball - a; b - a <= 100; a--, b++)
            {
                int beetles = MathUtils.MinimumCoinChange(a, Part3Stamps.AsSpan()).Length
                            + MathUtils.MinimumCoinChange(b, Part3Stamps.AsSpan()).Length;
                minBeetles = Math.Min(minBeetles, beetles);
            }
            totalBeetles += minBeetles;
        }
        LogAnswer(totalBeetles);
    }
    // ReSharper enable CognitiveComplexity

    /// <inheritdoc />
    protected override int ConvertLine(string line) => int.Parse(line);
}
