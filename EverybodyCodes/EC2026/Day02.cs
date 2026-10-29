using System.Collections.Immutable;
using Challenge.Solvers;
using Challenge.Solvers.Specialized;
using Challenge.Utils;
using Challenge.Utils.Extensions.Collections;

namespace EverybodyCodes.EC2026;

/// <summary>
/// Solver for 2026 Day 2
/// </summary>
[Solver(2026, 2)]
public sealed partial class Day2 : ArraySolver<Day2.BladeComparer>, IEverybodyCodesSolver
{
    public sealed class BladeComparer : IComparer<char>
    {
        private readonly Dictionary<Pair<char>, int> comparisons = [];

        public ImmutableArray<char> ValidBlades { get; }

        public BladeComparer(ReadOnlySpan<char> data)
        {
            HashSet<char> validBlades = [];
            foreach (Range split in data.Split(' '))
            {
                ReadOnlySpan<char> comparison = data[split];
                char x = comparison[0];
                char y = comparison[2];

                if (comparison[1] is '<')
                {
                    this.comparisons[(x, y)] =  1;
                    this.comparisons[(y, x)] = -1;
                }
                else
                {
                    this.comparisons[(x, y)] = -1;
                    this.comparisons[(y, x)] =  1;
                }

                validBlades.Add(x);
                validBlades.Add(y);
            }

            this.ValidBlades = [..validBlades];
            Queue<Pair<char>> missing = [];
            missing.EnqueueRange(this.ValidBlades.EnumeratePairs()
                                     .Select(p => (Pair<char>)p)
                                     .Where(p => !this.comparisons.ContainsKey(p)));
            while (missing.TryDequeue(out Pair<char> pair))
            {
                if (TryDeduceOrder(pair, out int order))
                {
                    this.comparisons[pair]          =  order;
                    this.comparisons[pair.Inverted] = -order;
                }
            }
        }

        /// <inheritdoc />
        public int Compare(char x, char y) => this.comparisons[(x, y)];

        private bool TryDeduceOrder(Pair<char> pair, out int order)
        {
            foreach (char blade in this.ValidBlades)
            {
                if (blade != pair.First
                 && blade != pair.Second
                 && this.comparisons.TryGetValue((pair.First, blade), out int a)
                 && this.comparisons.TryGetValue((blade, pair.Second), out int b)
                 && a == b)
                {
                    order = a;
                    return true;
                }
            }

            order = 0;
            return false;
        }
    }

    // ReSharper disable CognitiveComplexity
    /// <inheritdoc />
    [Part(1)]
    public void RunPart1()
    {
        Span<char> blades = stackalloc char[this.Data[0].ValidBlades.Length];
        this.Data[0].ValidBlades.AsSpan().CopyTo(blades);

        long checksum = 0L;
        foreach (BladeComparer comparer in this.Data)
        {
            blades.Sort(comparer);
            checksum += long.Parse(blades);
        }

        LogAnswer(checksum);
    }

    /// <inheritdoc />
    [Part(2)]
    public void RunPart2() => RunPart1();

    /// <inheritdoc />
    [Part(3)]
    public void RunPart3() => RunPart1();
    // ReSharper enable CognitiveComplexity

    /// <inheritdoc />
    protected override BladeComparer ConvertLine(string line) => new(line);
}
