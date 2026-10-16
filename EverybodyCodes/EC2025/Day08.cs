using System.Collections.Immutable;
using Challenge.Solvers;
using Challenge.Utils;
using Challenge.Utils.Extensions.Collections;
using Challenge.Utils.Extensions.Enumerables;
using Challenge.Utils.Extensions.Ranges;

namespace EverybodyCodes.EC2025;

/// <summary>
/// Solver for 2025 Day 8
/// </summary>
[Solver(2025, 8)]
public sealed partial class Day08 : Solver<ImmutableArray<int>>, IEverybodyCodesSolver
{
    // ReSharper disable CognitiveComplexity
    /// <inheritdoc />
    [Part(1)]
    public void RunPart1()
    {
        const int NAILS = 32;
        int crosses = this.Data.Pairwise()
                          .Count(p => p.First - 1 == ((p.Second - 1) + (NAILS / 2)) % NAILS);
        LogAnswer(crosses);
    }

    /// <inheritdoc />
    [Part(2)]
    public void RunPart2()
    {
        UnorderedPair<int>[] lines = [..this.Data.Pairwise().Select(p => (UnorderedPair<int>)p)];
        int crosses = lines.EnumeratePairs()
                           .Count(IsCrossing);
        LogAnswer(crosses);
    }

    /// <inheritdoc />
    [Part(3)]
    public void RunPart3()
    {
        const int NAILS = 256;
        UnorderedPair<int>[] lines = [..this.Data.Pairwise().Select(p => (UnorderedPair<int>)p)];
        int[] nails = [..(1..^NAILS)];
        int maxCuts = nails.EnumeratePairs()
                           .Max(c => lines.Count(l => IsCrossing((l, c))));
        LogAnswer(maxCuts);
    }
    // ReSharper enable CognitiveComplexity

    private static bool IsCrossing(UnorderedPair<UnorderedPair<int>> lines)
    {
        if (lines.First == lines.Second) return true;

        (int a, int b) = lines.Second;
        if (lines.First.HasValue(a) || lines.First.HasValue(b)) return false;

        (int start, int end) = lines.First;
        Range lineRange = start..^end;
        return lineRange.IsInRange(a) ^ lineRange.IsInRange(b);
    }

    /// <inheritdoc />
    protected override ImmutableArray<int> Convert(string[] rawInput) => ParseUtils.ParseImmutableArray(rawInput[0], ',', s => int.Parse(s));
}
