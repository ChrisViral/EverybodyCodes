using Challenge.Maths.Vectors;
using Challenge.Solvers;
using Challenge.Solvers.Specialized;
using Challenge.Utils.Extensions.Collections;
using ZLinq;

namespace EverybodyCodes.EC2024;

/// <summary>
/// Solver for 2024 Day 3
/// </summary>
[Solver(2024, 3)]
public sealed partial class Day03 : GridSolver<bool>, IEverybodyCodesSolver
{
    private const char FILLED = '#';

    // ReSharper disable CognitiveComplexity
    /// <inheritdoc />
    [Part(1)]
    public void RunPart1()
    {
        int removed = GetRemovedCount();
        LogAnswer(removed);
    }

    /// <inheritdoc />
    [Part(2)]
    public void RunPart2()
    {
        int removed = GetRemovedCount();
        LogAnswer(removed);
    }

    /// <inheritdoc />
    [Part(3)]
    public void RunPart3()
    {
        int removed = GetRemovedCount(options: AdjacentOptions.WITH_DIAGONALS);
        LogAnswer(removed);
    }
    // ReSharper enable CognitiveComplexity

    private int GetRemovedCount(AdjacentOptions options = AdjacentOptions.CARDINAL_ONLY)
    {
        HashSet<Vector2<int>> remaining = new(this.Grid.Size);
        foreach (Vector2<int> position in this.Grid.Dimensions.Enumerate())
        {
            if (this.Grid[position])
            {
                remaining.Add(position);
            }
        }

        int removed = 0;
        List<Vector2<int>> toRemove = new(remaining.Count);
        while (!remaining.IsEmpty)
        {
            removed += remaining.Count;
            toRemove.AddRange(remaining.Where(p => p.Adjacent(options)
                                                    .Any(a => !this.Grid.TryGetPosition(a, out bool isFilled) || !isFilled)));
            remaining.ExceptWith(toRemove);
            toRemove.ForEach(p => this.Grid[p] = false);
            toRemove.Clear();
        }

        return removed;
    }

    /// <inheritdoc />
    protected override bool[] LineConverter(string line) => line.Select(c => c is FILLED).ToArray();
}
