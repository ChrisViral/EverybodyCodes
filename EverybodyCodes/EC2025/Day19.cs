using Challenge.Solvers;
using Challenge.Solvers.Specialized;

namespace EverybodyCodes.EC2025;

/// <summary>
/// Solver for 2025 Day 19
/// </summary>
[Solver(2025, 19)]
public sealed partial class Day19 : ArraySolver<Day19.Gap>, IEverybodyCodesSolver
{
    public readonly record struct Gap
    {
        public int Position { get; }

        public int Start { get; }

        public Gap(ReadOnlySpan<char> data)
        {
            Span<Range> splits = stackalloc Range[3];
            data.Split(splits, ',', DEFAULT_OPTIONS);
            this.Position = int.Parse(data[splits[0]]);
            this.Start    = int.Parse(data[splits[1]]);
        }
    }

    // ReSharper disable CognitiveComplexity
    /// <inheritdoc />
    [Part(1)]
    public void RunPart1()
    {
        int maxDiagonalPos = this.Data.Max(g => g.Position + g.Start);
        int flaps = (maxDiagonalPos + 1) / 2;
        LogAnswer(flaps);
    }

    /// <inheritdoc />
    [Part(2)]
    public void RunPart2()
    {
        int furthestSeen = 0;
        int maxDiagonalPos = 0;
        foreach (Gap gap in this.Data)
        {
            if (gap.Position <= furthestSeen) continue;

            furthestSeen   = gap.Position;
            maxDiagonalPos = Math.Max(maxDiagonalPos, gap.Position + gap.Start);
        }

        int flaps = (maxDiagonalPos + 1) / 2;
        LogAnswer(flaps);
    }

    /// <inheritdoc />
    [Part(3)]
    public void RunPart3() => RunPart2();
    // ReSharper enable CognitiveComplexity

    /// <inheritdoc />
    protected override Gap ConvertLine(string line) => new(line);
}
