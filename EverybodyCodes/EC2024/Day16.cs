using System.Collections.Immutable;
using System.Text;
using Challenge.Collections;
using Challenge.Solvers;
using Challenge.Utils;
using Challenge.Utils.Extensions.Arrays;
using Challenge.Utils.Extensions.Numbers;
using Challenge.Utils.Extensions.Ranges;
using ZLinq;

namespace EverybodyCodes.EC2024;

/// <summary>
/// Solver for 2024 Day 16
/// </summary>
[Solver(2024, 16)]
public sealed partial class Day16 : Solver<ImmutableArray<Day16.Wheel>>, IEverybodyCodesSolver
{
    public readonly record struct Wheel(int Spins, ImmutableArray<string> Slots)
    {
        public string GetSlot(int turn, int offset = 0) => this.Slots[((this.Spins * turn) + offset).Mod(this.Slots.Length)];
    }

    /// <summary>
    /// Solver for 2024 Day 16
    /// </summary>
    public Day16() : base(options: StringSplitOptions.RemoveEmptyEntries) { }

    // ReSharper disable CognitiveComplexity
    /// <inheritdoc />
    [Part(1)]
    public void RunPart1()
    {
        const int TURNS = 100;
        int count = this.Data.Length;
        StringBuilder sequence = new((count * 4) - 1);
        sequence.AppendJoin(' ', this.Data.Select(w => w.GetSlot(TURNS)));
        LogAnswer(sequence.ToString());
    }

    /// <inheritdoc />
    [Part(2)]
    public void RunPart2()
    {
        const long TURNS = 202420242024L;
        int cycleLength = int.LCM(this.Data.Select(w => w.Slots.Length));

        int cycleScore = 0;
        int[] scores = new int[cycleLength];
        Counter<char> frequencies = new(this.Data.Length * 2);
        foreach (int i in ..cycleLength)
        {
            foreach (Wheel wheel in this.Data)
            {
                string slot = wheel.GetSlot(i + 1);
                frequencies.Add(slot[0]);
                frequencies.Add(slot[2]);
            }

            int score   = frequencies.Counts.Where(c => c >= 3).Sum(c => c - 2);
            scores[i]   = score;
            cycleScore += score;
            frequencies.Clear();
        }

        (long cycles, long remaining) = Math.DivRem(TURNS, cycleLength);
        long totalScore = cycleScore * cycles;
        totalScore += scores.AsSpan(0, (int)remaining).Sum();
        LogAnswer(totalScore);
    }

    /// <inheritdoc />
    [Part(3)]
    public void RunPart3()
    {
        const int TURNS = 256;
        Counter<char> frequencies = new(this.Data.Length * 2);
        Dictionary<(int turn, int offset), (int min, int max)> cache = new(TURNS);
        (int min, int max) = GetMinMaxScore();
        LogAnswer($"{max} {min}");
        return;

        (int min, int max) GetMinMaxScore(int turn = 0, int offset = 0)
        {
            (int, int) cacheKey = (turn, offset);
            if (cache.TryGetValue(cacheKey, out (int, int) cached)) return cached;

            int score = 0;
            if (turn is not 0)
            {
                foreach (Wheel wheel in this.Data)
                {
                    string slot = wheel.GetSlot(turn, offset);
                    frequencies.Add(slot[0]);
                    frequencies.Add(slot[2]);
                }

                score = frequencies.Counts.Where(c => c >= 3).Sum(c => c - 2);
                frequencies.Clear();

                if (turn is TURNS)
                {
                    cache[cacheKey] = (score, score);
                    return (score, score);
                }
            }

            int minScore = int.MaxValue;
            int maxScore = int.MinValue;
            for (int leftPull = -1; leftPull <= 1; leftPull++)
            {
                (int nextMin, int nextMax) = GetMinMaxScore(turn + 1, offset + leftPull);
                minScore = Math.Min(minScore, score + nextMin);
                maxScore = Math.Max(maxScore, score + nextMax);
            }

            cache[cacheKey] = (minScore, maxScore);
            return (minScore, maxScore);
        }
    }
    // ReSharper enable CognitiveComplexity

    /// <inheritdoc />
    protected override ImmutableArray<Wheel> Convert(string[] rawInput)
    {
        // Get wheel spins
        int[] wheelSpins = ParseUtils.ParseArray(rawInput[0], ',', s => int.Parse(s));
        int wheelCount   = wheelSpins.Length;

        // Get individual wheels
        ImmutableArray<string>.Builder[] wheels = new ImmutableArray<string>.Builder[wheelCount];
        wheels.Fill(() => ImmutableArray.CreateBuilder<string>(rawInput.Length - 1));
        foreach (ReadOnlySpan<char> row in rawInput.AsSpan(1))
        {
            int end = (row.Length + 1) / 4;
            foreach (int i in ..end)
            {
                ReadOnlySpan<char> data = row.Slice(i * 4, 3);
                if (!data.IsWhiteSpace())
                {
                    wheels[i].Add(data.ToString());
                }
            }
        }

        return wheelSpins.Index().Select(p => new Wheel(p.Item, wheels[p.Index].ToImmutable())).ToImmutableArray();
    }
}
