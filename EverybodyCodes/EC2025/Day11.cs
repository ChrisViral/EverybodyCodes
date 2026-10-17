using Challenge.Solvers;
using Challenge.Solvers.Specialized;
using Challenge.Utils;
using Challenge.Utils.Extensions.Enumerables;
using ZLinq;

namespace EverybodyCodes.EC2025;

/// <summary>
/// Solver for 2025 Day 11
/// </summary>
[Solver(2025, 11)]
public sealed partial class Day11 : ArraySolver<long>, IEverybodyCodesSolver
{
    // ReSharper disable CognitiveComplexity
    /// <inheritdoc />
    [Part(1)]
    public void RunPart1()
    {
        const long MAX_ROUNDS = 10L;

        long rounds = 0L;
        SimulatePhase1(ref rounds, MAX_ROUNDS);
        SimulatePhase2(rounds, MAX_ROUNDS);
        long checksum = this.Data.Index().Sum(p => (p.Index + 1) * p.Item);
        LogAnswer(checksum);
    }

    /// <inheritdoc />
    [Part(2)]
    public void RunPart2()
    {
        long rounds = 0L;
        SimulatePhase1(ref rounds);
        rounds += CalculatePhase2Time();
        LogAnswer(rounds);
    }

    /// <inheritdoc />
    [Part(3)]
    public void RunPart3()
    {
        long rounds = CalculatePhase2Time();
        LogAnswer(rounds);
    }
    // ReSharper enable CognitiveComplexity return string.Empty;

    private void SimulatePhase1(ref long rounds, long maxRounds = long.MaxValue)
    {
        bool moved;
        for (moved = true; moved && rounds < maxRounds; rounds++)
        {
            moved = false;
            foreach (Pair<int> pair in Enumerable.Range(0, this.Data.Length).Pairwise())
            {
                ref long first  = ref this.Data[pair.First];
                ref long second = ref this.Data[pair.Second];
                if (second < first)
                {
                    first--;
                    second++;
                    moved = true;
                }
            }
        }

        if (!moved)
        {
            rounds--;
        }
    }

    private void SimulatePhase2(long rounds, long maxRounds = long.MaxValue)
    {
        for (; rounds < maxRounds; rounds++)
        {
            foreach (Pair<int> pair in Enumerable.Range(0, this.Data.Length).Pairwise())
            {
                ref long first  = ref this.Data[pair.First];
                ref long second = ref this.Data[pair.Second];
                if (first < second)
                {
                    first++;
                    second--;
                }
            }
        }
    }

    private long CalculatePhase2Time()
    {
        long average = this.Data.Sum() / this.Data.Length;
        return this.Data.Where(v => v > average)
                   .Sum(v => v - average);
    }

    /// <inheritdoc />
    protected override long ConvertLine(string line) => long.Parse(line);
}
