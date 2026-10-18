using Challenge.Collections;
using Challenge.Maths.Vectors;
using Challenge.Solvers;
using Challenge.Utils.Extensions.Numbers;
using Challenge.Utils.Extensions.Ranges;
using Challenge.Utils.Extensions.Spans;
using CommunityToolkit.HighPerformance;
using ZLinq;

namespace EverybodyCodes.EC2025;

/// <summary>
/// Solver for 2025 Day 14
/// </summary>
[Solver(2025, 14)]
public sealed partial class Day14 : Solver<DelayedGrid<bool>>, IEverybodyCodesSolver
{
    private readonly record struct PatternData(int Round, int Active);

    private const char INACTIVE = '.';
    private const char ACTIVE   = '#';

    // ReSharper disable CognitiveComplexity
    /// <inheritdoc />
    [Part(1)]
    public void RunPart1()
    {
        const int ROUNDS = 10;
        int totalActiveCount = SimulateRounds(this.Data, ROUNDS);
        LogAnswer(totalActiveCount);
    }

    /// <inheritdoc />
    [Part(2)]
    public void RunPart2()
    {
        const int ROUNDS = 2025;
        int totalActiveCount = SimulateRounds(this.Data, ROUNDS);
        LogAnswer(totalActiveCount);
    }

    /// <inheritdoc />
    [Part(3)]
    public void RunPart3()
    {
        const int GRID_SIZE = 34;
        const int ROUNDS = 1000000000;
        DelayedGrid<bool> grid = new(GRID_SIZE, GRID_SIZE, b => (b ? ACTIVE : INACTIVE).ToString());

        int centreWidth  = this.Data.Width;
        int centreHeight = this.Data.Height;
        int centreX = (GRID_SIZE - centreWidth) / 2;
        int centreY = (GRID_SIZE - centreHeight) / 2;

        int round = 0;
        int activeCount = 0;
        string pattern = string.Empty;
        Dictionary<string, PatternData> patterns = [];
        while (round <= ROUNDS && (string.IsNullOrEmpty(pattern) || patterns.TryAdd(pattern, new PatternData(round, activeCount))))
        {
            round++;
            activeCount = SimulateRound(grid);
            Span2D<bool> centre = grid.AsSpan2D(centreX, centreWidth, centreY, centreHeight);
            pattern = centre.SequenceEqual(this.Data) ? grid.ToString() : string.Empty;
        }

        int totalActiveCount = patterns.Values.Sum(p => p.Active);
        int roundsRemaining = ROUNDS - round - 1;

        PatternData firstPattern = patterns[pattern];
        PatternData[] validPatterns =
        [
            ..patterns.Values
                      .Where(p => p.Round >= firstPattern.Round)
                      .Select(p => p with { Round = p.Round - firstPattern.Round })
                      .OrderBy(p => p.Round)
        ];

        int cycleLength = round - firstPattern.Round;
        int cycleActiveCount = validPatterns.Sum(p => p.Active);
        (int cycles, roundsRemaining) = Math.DivRem(roundsRemaining, cycleLength);

        totalActiveCount += cycles * cycleActiveCount;
        totalActiveCount += validPatterns.TakeWhile(p => p.Round <= roundsRemaining).Sum(p => p.Active);
        LogAnswer(totalActiveCount);
    }
    // ReSharper enable CognitiveComplexity

    private static int SimulateRounds(DelayedGrid<bool> grid, int rounds)
    {
        int totalActivecount = 0;
        foreach (int _ in ..rounds)
        {
            totalActivecount += SimulateRound(grid);
        }

        return totalActivecount;
    }

    private static int SimulateRound(DelayedGrid<bool> grid)
    {
        int roundActiveTotal = 0;
        foreach ((Vector2<int> position, bool active) in grid.EnumeratePositions())
        {
            int activeCount = position.Adjacent(AdjacentOptions.DIAGONAL_ONLY).Count(a => grid.IsPositionEqual(a, true));
            bool shouldBeActive = activeCount.IsEven != active;
            grid[position] = shouldBeActive;
            if (shouldBeActive)
            {
                roundActiveTotal++;
            }
        }

        grid.Apply();
        return roundActiveTotal;
    }

    /// <inheritdoc />
    protected override DelayedGrid<bool> Convert(string[] rawInput)
    {
        int width = rawInput[0].Length;
        int height = rawInput.Length;
        DelayedGrid<bool> grid = new(width, height, rawInput,
                                     l => l.Select(c => c is ACTIVE).ToArray(),
                                     b => (b ? ACTIVE : INACTIVE).ToString());
        return grid;
    }
}
