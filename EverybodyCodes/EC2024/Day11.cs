using System.Collections.Frozen;
using System.Collections.Immutable;
using Challenge.Collections;
using Challenge.Solvers;
using Challenge.Utils;
using Challenge.Utils.Extensions.Ranges;
using Challenge.Utils.Extensions.Spans;
using ZLinq;

namespace EverybodyCodes.EC2024;

/// <summary>
/// Solver for 2024 Day 11
/// </summary>
[Solver(2024, 11)]
public sealed partial class Day11 : Solver<FrozenDictionary<string, ImmutableArray<string>>>, IEverybodyCodesSolver
{
    private static readonly Counter<string, long> CurrentDay = [];
    private static readonly Counter<string, long> NextDay    = [];

    // ReSharper disable CognitiveComplexity
    /// <inheritdoc />
    [Part(1)]
    public void RunPart1()
    {
        const int DAYS = 4;
        const string STARTER = "A";
        long result = SimulateTermites(DAYS, STARTER);
        LogAnswer(result);
    }

    /// <inheritdoc />
    [Part(2)]
    public void RunPart2()
    {
        const int DAYS = 10;
        const string STARTER = "Z";
        long result = SimulateTermites(DAYS, STARTER);
        LogAnswer(result);
    }

    /// <inheritdoc />
    [Part(3)]
    public void RunPart3()
    {
        const int DAYS = 20;

        long minTermites = long.MaxValue;
        long maxTermites = long.MinValue;
        foreach (string starter in this.Data.Keys)
        {
            long termites = SimulateTermites(DAYS, starter);
            minTermites = Math.Min(minTermites, termites);
            maxTermites = Math.Max(maxTermites, termites);
        }

        LogAnswer(maxTermites - minTermites);
    }
    // ReSharper enable CognitiveComplexity

    private long SimulateTermites(int days, string starter)
    {
        Counter<string, long> currentDay = CurrentDay;
        Counter<string, long> nextDay    = NextDay;
        currentDay[starter] = 1L;
        foreach (int _ in ..days)
        {
            foreach (string current in this.Data.Keys)
            {
                long count = currentDay[current];
                if (count is 0) continue;

                ImmutableArray<string> map = this.Data[current];
                foreach (string target in map)
                {
                    nextDay[target] += count;
                }
            }

            currentDay.Clear();
            SwapUtils.Swap(ref currentDay, ref nextDay);
        }

        long result = currentDay.Counts.Sum();
        currentDay.Clear();
        return result;
    }

    /// <inheritdoc />
    protected override FrozenDictionary<string, ImmutableArray<string>> Convert(string[] rawInput)
    {
        Dictionary<string, ImmutableArray<string>> map = [];
        foreach (string line in rawInput)
        {
            int colonIndex = line.IndexOf(':');
            map[line[..colonIndex]] = line.AsSpan(colonIndex + 1)
                                          .Split(',')
                                          .AsValueEnumerable()
                                          .Select(r => line.AsSpan(colonIndex + 1)[r].ToString())
                                          .ToImmutableArray();
        }
        return map.ToFrozenDictionary();
    }
}
