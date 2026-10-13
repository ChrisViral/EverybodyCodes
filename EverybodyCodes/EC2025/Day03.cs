using System.Collections.Immutable;
using Challenge.Collections;
using Challenge.Solvers;
using Challenge.Utils;
using Challenge.Utils.Extensions.Enumerables;

namespace EverybodyCodes.EC2025;

/// <summary>
/// Solver for 2025 Day 3
/// </summary>
[Solver(2025, 3)]
public sealed partial class Day03 : Solver<ImmutableArray<int>>, IEverybodyCodesSolver
{
    // ReSharper disable CognitiveComplexity
    /// <inheritdoc />
    [Part(1)]
    public void RunPart1()
    {
        int result = this.Data.OrderDescending()
                         .Pairwise()
                         .Where(p => p.First > p.Second)
                         .SelectMany(p => p.Enumerate())
                         .Distinct()
                         .Sum();
        LogAnswer(result);
    }

    /// <inheritdoc />
    [Part(2)]
    public void RunPart2()
    {
        const int TAKE_COUNT = 20;
        int result = this.Data.OrderDescending()
                         .Pairwise()
                         .Where(p => p.First > p.Second)
                         .TakeLast(TAKE_COUNT - 1)
                         .SelectMany(p => p.Enumerate())
                         .Distinct()
                         .Sum();
        LogAnswer(result);
    }

    /// <inheritdoc />
    [Part(3)]
    public void RunPart3()
    {
        int sets = 0;
        HashSet<int> set = new(this.Data.Length);
        Counter<int> remaining = new(this.Data);

        do
        {
            set.UnionWith(remaining.EnumerateAll()
                                   .OrderDescending()
                                   .Pairwise()
                                   .Where(p => p.First > p.Second)
                                   .SelectMany(p => p.Enumerate()));

            if (set.Count is 0)
            {
                sets += remaining.Counts.Sum();
                remaining.Clear();
            }
            else
            {
                set.ForEach(v => remaining.Remove(v));
                set.Clear();
                sets++;
            }
        }
        while (remaining.Count > 0);

        LogAnswer(sets);
    }
    // ReSharper enable CognitiveComplexity

    /// <inheritdoc />
    protected override ImmutableArray<int> Convert(string[] rawInput) => ParseUtils.ParseImmutableArray(rawInput[0], ',', s => int.Parse(s));
}
